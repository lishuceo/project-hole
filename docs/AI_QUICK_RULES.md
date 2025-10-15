# 🤖 AI Quick Rules for WasiCore Development

> **如果你是AI，这是你开发WasiCore游戏前必须读的第一个文档**
> 
> **This document contains essential rules for AI agents developing WasiCore games. These rules are designed to be included in system prompts to prevent common mistakes.**

## 🚀 3分钟快速开始

**如果你第一次开发WasiCore游戏，按此顺序：**

### 步骤1：阅读本文档的规则部分（2分钟）
向下滚动阅读5个关键规则

### 步骤2：选择技术栈（1分钟）
阅读规则3的快速参考表，确定用Panel/Canvas/Shape/ECS

或者查阅：[GAME_TYPE_GUIDE.md](GAME_TYPE_GUIDE.md)（更详细）

### 步骤3：阅读示例代码（10-15分钟）
根据规则3的表格，读取对应的示例代码

### 遇到问题时查阅：
- 编译/运行时错误 → 本文档末尾的"常见错误速查"章节
- 技术栈详情 → [GAME_TYPE_GUIDE.md](GAME_TYPE_GUIDE.md)
- UI学习路径 → [UI_LEARNING_PATH.md](UI_LEARNING_PATH.md) ⭐ UI主入口
- UI详细说明 → [guides/AI_FRIENDLY_UI_API.md](guides/AI_FRIENDLY_UI_API.md)

---

## 🚨 Critical Rules

### 1. .NET Framework Version
- **ALWAYS use .NET 9.0** for all projects
- Ensure `<TargetFramework>net9.0</TargetFramework>` in .csproj files
- Never target older versions like net8.0 or net6.0

### 2. API Usage and Compilation Errors
When encountering API issues or compilation errors:
- **NEVER guess API names or signatures**
- **ALWAYS search in `api-client-reference/` for client-side APIs**
- **ALWAYS search in `api-server-reference/` for server-side APIs**
- Use `grep` tool to search XML files for exact API definitions

**Example workflow:**
```bash
# Error: 'Unit' does not contain a definition for 'ApplyDamage'
# Step 1: Search for damage-related APIs
grep -i "damage" api-client-reference/*.xml

# Step 2: Found TakeDamage in GameCore.xml
# Step 3: Check exact signature
grep -B2 -A5 "TakeDamage" api-client-reference/GameCore.xml

# Step 4: Use correct API
# unit.TakeDamage(damage, source) ✅
```

#### Common API Mistakes from Other Frameworks

**从WPF/Unity迁移的常见错误：**

| 你可能会写（错误） | WasiCore正确写法 | 原因 |
|------------------|----------------|------|
| `label.Foreground = ...` | `label.TextColor = ...` | 属性名不同 |
| `panel.Visibility = Visibility.Hidden` | `panel.Visible = false` | 类型不同（bool vs枚举） |
| `rect.Fill = Brushes.Red` | `panel.Background = new SolidColorBrush(Color.Red)` | Rectangle不是UI控件 |
| `canvas.Children.Add(...)` | `canvas.FillRectangle(...)` | Canvas用绘制API |
| `var x = panel.Position.X` | 自己维护`float x`变量 | UIPosition没有X/Y属性 |
| `Task.Delay(1000)` | `Game.Delay(TimeSpan.FromSeconds(1))` | WebAssembly安全 |

#### Event Handler Rules

**Trigger事件处理器必须返回bool：**
```csharp
// ❌ 错误：忘记返回值
Trigger<EventGameKeyDown> trigger = new(async (s, d) =>
{
    HandleInput(d.Key);
    // 忘记return！编译错误
});

// ✅ 正确
Trigger<EventGameKeyDown> trigger = new(async (s, d) =>
{
    HandleInput(d.Key);
    return true;  // 必须返回bool
});
```

#### Think Method DeltaTime

**Think方法的deltaTime单位是毫秒：**
```csharp
// ❌ 错误：直接使用deltaTime
public void Think(int deltaTime)
{
    playerX += velocity * deltaTime; // 速度会非常快！
}

// ✅ 正确：转换为秒
public void Think(int deltaTime)
{
    UpdateGame(deltaTime / 1000f); // 转换为秒
}

private void UpdateGame(float deltaTimeSeconds)
{
    playerX += velocity * deltaTimeSeconds; // 现在单位正确
}
```

### 3. Choose the Right Tech Stack (2D vs 3D)

**FIRST: Determine if your game is 2D or 3D**

#### 🎮 2D Game Keywords
If the user mentions: 横版、卷轴、像素、平台跳跃、棋盘、卡牌、拼图、消除
Or classic games: 超级玛丽、俄罗斯方块、贪吃蛇、五子棋、Flappy Bird

→ Use **2D Implementation**

#### 🎮 3D Game Keywords  
If the user mentions: 第一人称、第三人称、3D场景、物理模拟、立体
Or game genres: ARPG、塔防、生存游戏、模拟游戏

→ Use **3D Implementation**

---

### 3.1. 2D Game Implementation Paths

**Choose based on rendering complexity:**

#### 🟢 Simple 2D (Panel-based) - RECOMMENDED for most 2D games
**Use when:** Simple shapes, grid-based, turn-based games  
**Examples:** `Game2048/`, `Gomoku/`, `FlappyBirdGame/`  
**Tech stack:**
```csharp
// ⚠️ IMPORTANT: Maintain your own position variables
private float characterX = 100f;
private float characterY = 200f;

// Create game objects as Panels
var character = new Panel {
    Width = 32, Height = 32,
    Position = new UIPosition((int)characterX, (int)characterY),
    PositionType = UIPositionType.Absolute,
    Background = new SolidColorBrush(Color.Red)
};
gameArea.AddChild(character);

// Update position (update your variables first, then Panel)
characterX += velocityX * deltaTime;
characterY += velocityY * deltaTime;
character.Position = new UIPosition((int)characterX, (int)characterY);

// ❌ WRONG: UIPosition has NO X/Y properties
var x = character.Position.X;  // Compilation ERROR!
var y = character.Position.Y;  // Compilation ERROR!
```

#### 🟡 Advanced 2D (Canvas-based)
**Use when:** Custom drawing, particles, gradients, complex animations  
**Examples:** `FlappyBirdGame/`  
**Tech stack:**
```csharp
// Canvas drawing API - every frame
gameCanvas.ResetState(); // ⚠️ MUST call first to clear canvas
gameCanvas.FillPaint = new SolidPaint(Color.Red);
gameCanvas.FillRectangle(x, y, width, height);

// ❌ WRONG: Canvas.Children is READ-ONLY
gameCanvas.Children.Clear();  // Compilation ERROR!
gameCanvas.Children.Add(...); // Compilation ERROR!

// ✅ CORRECT: Use drawing methods
gameCanvas.FillRectangle(...);
gameCanvas.DrawLine(...);
```

**⚠️ Important for 2D games:**
- Panel/Canvas games **DO NOT need camera setup**
- Panel Position is screen coordinates (no 3D camera needed)
- Canvas drawing is screen coordinates (no 3D camera needed)

---

### 3.2. 3D Game Implementation Paths

#### 🔵 Simple 3D (Shape-based)
**Use when:** Prototype, simple 3D objects, no complex game logic  
**Examples:** `JumpJumpGame/`, `PrimitiveShapeTest/`  
**Tech stack:**
```csharp
var actor = AIShapeFactory.CreateShape(
    PrimitiveShape.Cube,
    new ScenePoint(new Vector3(x, y, z), Game.LocalScene),
    scale, null
);
```

#### 🔷 Advanced 3D (ECS-based)
**Use when:** Units, abilities, items, complex game systems  
**Examples:** `ARPGTemplate/`, `VampireSurvivors3D/`, `TowerDefenseGame/`  
**Tech stack:**
```csharp
var unitData = UnitLinks.MyHero.Data;
var unit = unitData.CreateUnit(player, position);
```

---

### 3.3. Quick Reference Table

| Game Type | User Says | Use This Example | Tech Stack |
|-----------|-----------|------------------|------------|
| 2D 横版跳跃 | 超级玛丽、Celeste | **SuperMarioSimple** ✅ | Panel-based |
| 2D 飞行躲避 | Flappy Bird、直升机 | **FlappyBird** | Canvas-based |
| 2D 网格拼图 | 2048、俄罗斯方块 | **Game2048** | Panel-based |
| 2D 棋盘游戏 | 五子棋、象棋 | **Gomoku** | Panel-based |
| 3D 平台跳跃 | 跳一跳、神庙逃亡 | **JumpJump** | Shape-based |
| 3D 生存游戏 | 吸血鬼幸存者 | **VampireSurvivors3D** | ECS-based |
| 3D 塔防游戏 | 塔防、守卫 | **TowerDefenseGame** | ECS-based |

**Example workflow:**
```bash
# User: "做一个超级玛丽"
# Step 1: 识别 → 2D横版跳跃
# Step 2: 选择 → SuperMarioSimple (Panel-based)
# Step 3: 复制 → read_file("src/SuperMarioGame/SuperMarioSimple.cs")
# Step 4: 修改 → 调整游戏逻辑
```

### 4. Game Mode Definition and Configuration
When creating a new game, you MUST follow these 3 steps IN ORDER:

1. **Define a GameLink in ScopeData.cs:**
   ```csharp
   // In ScopeData.cs -> public static class GameMode
   public static readonly GameLink<GameDataGameMode> MyNewGame = new("MyNewGame"u8);
   ```

2. **Create the GameDataGameMode IN ScopeData.cs (NOT in your game class):**
   ```csharp
   // In ScopeData.cs -> OnRegisterGameClass() method
   // 🚨 CRITICAL: Create HERE in ScopeData.cs, NOT in your game class!
   _ = new GameDataGameMode(GameMode.MyNewGame)
   {
       Name = "My New Game",
       Gameplay = Gameplay.Default,
       PlayerSettings = PlayerSettings.Default,
       SceneList = [Scene.YourScene],  // Use a dedicated scene
       DefaultScene = Scene.YourScene,
   };
   ```

3. **Register in GlobalConfig.cs:**
   ```csharp
   // Add to AvailableGameModes dictionary
   GameDataGlobalConfig.AvailableGameModes = new()
   {
       // ... existing modes ...
       {"MyNewGame", ScopeData.GameMode.MyNewGame},
   };
   
   // Set as default test mode
   GameDataGlobalConfig.TestGameMode = ScopeData.GameMode.MyNewGame;
   ```

### 5. Scene Configuration for New Games
When creating a new game mode, also create a dedicated Scene:

1. **Define Scene Link in ScopeData.cs:**
   ```csharp
   // In ScopeData.cs -> public static class Scene
   public static readonly GameLink<GameDataScene> MyGameScene = new("my_game_scene"u8);
   ```

2. **Create Scene Data in ScopeData.cs:**
   ```csharp
   // In ScopeData.cs -> OnRegisterGameClass() method
   _ = new GameDataScene(Scene.MyGameScene)
   {
       Name = "My Game Scene",
       HostedSceneTag = new HostedSceneTag("my_game_scene"u8, "new_scene_1"u8),
       Size = new(64 * 256, 64 * 256),
       PlacedPlayerObjects = [] // ⚠️ Empty = clean scene, let game create its own objects
   };
   ```

⚠️ **Common Mistake**: Using `Scene.DefaultScene` which contains pre-placed test units that will appear in your game!

## 📋 Complete Reference Template

### Step-by-Step Checklist for Creating a New Game

#### ✅ Step 1: Edit ScopeData.cs - Add GameMode and Scene Links
```csharp
// In ScopeData.cs -> public static class GameMode
public static readonly GameLink<GameDataGameMode> MyGame = new("MyGame"u8);

// In ScopeData.cs -> public static class Scene
public static readonly GameLink<GameDataScene> MyGameScene = new("my_game_scene"u8);
```

#### ✅ Step 2: Edit ScopeData.cs - Create GameData instances
```csharp
// In ScopeData.cs -> OnRegisterGameClass() method
// Around line 1000-1200 where other GameDataGameMode instances are

// Create Scene (put near other GameDataScene instances around line 900)
_ = new GameDataScene(Scene.MyGameScene)
{
    Name = "My Game Scene",
    HostedSceneTag = new HostedSceneTag("my_game_scene"u8, "new_scene_1"u8),
    Size = new(64 * 256, 64 * 256),
    PlacedPlayerObjects = [] // Empty = clean scene
};

// Create GameMode (put near other GameDataGameMode instances around line 1150)
_ = new GameDataGameMode(GameMode.MyGame)
{
    Name = "My Game",
    Gameplay = Gameplay.Default,
    PlayerSettings = PlayerSettings.Default,
    SceneList = [Scene.MyGameScene],
    DefaultScene = Scene.MyGameScene,
};
```

#### ✅ Step 3: Edit GlobalConfig.cs - Register GameMode
```csharp
GameDataGlobalConfig.AvailableGameModes = new()
{
    // ... existing modes ...
    {"MyGame", ScopeData.GameMode.MyGame},
};

GameDataGlobalConfig.TestGameMode = ScopeData.GameMode.MyGame;
```

#### ✅ Step 4: Create your game class
```csharp
// In src/MyGame/MyGame.cs
public class MyGame : IGameClass, IThinker
{
    public static void OnRegisterGameClass()
    {
        Game.OnGameTriggerInitialization += RegisterAll;
    }

    public static void RegisterAll()
    {
        if (Game.GameModeLink != ScopeData.GameMode.MyGame) return;
        
        Trigger<EventGameStart> trigger = new(async (s, d) =>
        {
            var game = new MyGame();
            game.Initialize();
            return true;
        });
        trigger.Register(Game.Instance);
    }
    
    public void Initialize() { /* ... */ }
    public void Think(int deltaTime) { /* ... */ }
}
```

---

## 🚨 UI事件和坐标系统（重要！）

### UI坐标类型差异

**UIPosition vs Vector2/3的关键区别：**

```csharp
// ❌ 错误：UIPosition没有X/Y属性
var uiPos = e.PointerPosition.Value;  // UIPosition类型
var x = uiPos.X;  // 编译错误！
var y = uiPos.Y;  // 编译错误！

// ✅ 正确：UIPosition使用Left/Top
var x = uiPos.Left;  // ✅ UI横坐标
var y = uiPos.Top;   // ✅ UI纵坐标

// Vector2/Vector3使用X/Y/Z
var worldPos = new Vector3(10, 20, 30);
var x = worldPos.X;  // ✅ 正确
```

### UI事件获取坐标的方式

#### 方式1：PointerEventArgs（控件事件）
```csharp
using GameUI.Control.Struct;  // ⚠️ 必需的命名空间

button.OnPointerPressed += (s, e) =>
{
    // ✅ PointerEventArgs有PointerPosition属性
    if (e.PointerPosition.HasValue)
    {
        var pos = e.PointerPosition.Value;  // UIPosition
        float x = pos.Left;  // 使用Left/Top
        float y = pos.Top;
    }
    
    // e.PointerButtons - 哪个按钮被按下
};
```

#### 方式2：全局事件（Trigger）
```csharp
using GameUI.TriggerEvent;  // EventGamePointerButton*事件

var trigger = new Trigger<EventGamePointerButtonUp>(async (s, e) =>
{
    if (e.PointerPosition.HasValue)
    {
        var pos = e.PointerPosition.Value;
        HandleClick(pos.Left, pos.Top);
    }
    return true;
});
trigger.Register(Game.Instance);
```

#### 方式3：InputManager（实时追踪鼠标）
```csharp
using GameUI.Device;

GameUI.Device.DeviceInfo.PrimaryInputManager.OnPointerButtonMove += (e) =>
{
    if (e.PointerPosition.HasValue)
    {
        var pos = e.PointerPosition.Value;
        // 实时更新鼠标位置
    }
};
```

### Canvas绘图必须事项

```csharp
// ❌ 错误：只调用Clear()
canvas.Clear();
// 可能出现残留、颜色错乱

// ✅ 正确：Clear() + ResetState()
canvas.Clear();       // 清空内容
canvas.ResetState();  // 重置画笔、颜色、变换等状态

// 然后绘制新内容
canvas.FillPaint = new SolidPaint(Color.Red);
canvas.FillRectangle(x, y, width, height);
```

### 事件命名规范

⚠️ 控件事件 vs 全局事件命名不同：

| 控件事件 | 全局事件类型 | 参数类型 |
|---------|------------|---------|
| `OnPointerPressed` | `EventGamePointerButtonDown` | PointerEventArgs |
| `OnPointerReleased` | `EventGamePointerButtonUp` | PointerEventArgs |
| ❌ 不存在 | `EventGamePointerButtonMove` | EventGamePointerButtonMove |
| `OnPointerCapturedMove` | ❌ 没有全局等价 | PointerCapturedMoveEventArgs |

**注意事项：**
- Canvas/Panel **不支持** `OnPointerMoved` 事件
- 移动追踪使用 `InputManager.OnPointerButtonMove` 或指针捕获
- 事件名是 `EventGamePointerButtonMove`（不是 `EventGamePointerMoved`）

---

## 🚨 常见错误速查与诊断

### 问题1：游戏角色看不见

**快速诊断清单：**
```
□ 是2D还是3D游戏？
  └─ 2D (Panel/Canvas)
      ├─ Panel Position是否在屏幕内？(0-800, 0-600)
      ├─ Visible = true？
      ├─ 是否调用了gameArea.AddChild()？
      └─ 是否调用了gameArea.AddToVisualTree()？
      
  └─ 3D (Shape/ECS)
      ├─ 是否调用了SetupCamera()？
      ├─ 相机角度是否正确？(如 -90, -70, 0)
      └─ Vector3坐标正确？(Z是高度，不是Y)

□ 是否使用了DefaultScene导致有预设单位遮挡？
  → 解决：规则5 - 创建专用Scene
  
□ Canvas是否调用了绘制方法？
  → 检查Think()中是否调用DrawGame()
  → 检查是否调用了ResetState()
```

### 问题2：编译错误 WASI015 (GameMode未创建)
→ **原因：** GameMode定义了但未创建GameDataGameMode实例  
→ **解决：** 规则4 - 在ScopeData.cs的OnRegisterGameClass()中创建

### 问题3：canvas.Children.Add() 编译错误
→ **原因：** Canvas.Children是只读的，不能Add/Clear  
→ **解决：** 规则3.1 - 使用canvas.FillRectangle()等绘制API  
→ **详细参考：** [guides/AI_FRIENDLY_UI_API.md](guides/AI_FRIENDLY_UI_API.md)

### 问题4：panel.Position.X 编译错误
→ **原因：** UIPosition没有X/Y属性  
→ **解决：** 规则3.1 - 自己维护float x, y变量  
→ **详细参考：** [guides/AI_FRIENDLY_UI_API.md](guides/AI_FRIENDLY_UI_API.md)

### 问题5：游戏有不期望的测试单位/UI
→ **原因：** 使用了DefaultScene，它包含预设单位  
→ **解决：** 规则5 - 创建专用Scene，设置PlacedPlayerObjects = []

### 问题6：Task.Delay 编译错误 (WASI001)
→ **原因：** WebAssembly环境不支持Task.Delay  
→ **解决：** 规则2 - 使用Game.Delay(TimeSpan.FromSeconds(...))

### 问题7：Trigger事件处理器编译错误
→ **原因：** 忘记返回bool值  
→ **解决：** 规则2 - 事件处理器必须return true/false

### 问题8：角色移动速度异常（非常快或非常慢）
→ **原因：** Think的deltaTime是毫秒，未转换为秒  
→ **解决：** 规则2 - Think(int deltaTime) → UpdateGame(deltaTime / 1000f)

### 问题9：UIPosition.X 或 UIPosition.Y 编译错误
→ **原因：** UIPosition使用Left/Top，不是X/Y  
→ **解决：** 使用 `uiPos.Left` 和 `uiPos.Top`  
→ **详细参考：** 本文档"UI事件和坐标系统"章节

### 问题10：canvas.PointerMoved 编译错误
→ **原因：** Canvas不支持PointerMoved事件  
→ **解决方案A：** 使用 `canvas.OnPointerPressed`（只有按下事件）  
→ **解决方案B：** 使用全局 `InputManager.OnPointerButtonMove`（追踪移动）  
→ **详细参考：** 本文档"UI事件和坐标系统"章节

### 问题11：Canvas绘制有残留或颜色错乱
→ **原因：** 只调用了Clear()，没有调用ResetState()  
→ **解决：** 每帧绘制前调用 `canvas.Clear()` 和 `canvas.ResetState()`

### 问题12：Canvas绘制网格有黑色间隙
→ **原因：** 像素对齐或抗锯齿问题  
→ **解决：** 矩形尺寸+1像素确保完全覆盖  
→ **示例：** `canvas.FillRectangle(x, y, SIZE + 1, SIZE + 1);`

### 问题13：找不到PointerEventArgs类型
→ **原因：** 缺少命名空间  
→ **解决：** 添加 `using GameUI.Control.Struct;`

---

## 📚 延伸阅读

**需要更多细节时查阅：**
- [GAME_TYPE_GUIDE.md](GAME_TYPE_GUIDE.md) - 详细的技术栈选择指南和决策流程
- [UI_LEARNING_PATH.md](UI_LEARNING_PATH.md) - GameUI系统学习路径与文档索引 ⭐ **UI主入口**
- [guides/AI_FRIENDLY_UI_API.md](guides/AI_FRIENDLY_UI_API.md) - UI系统详细说明和流式API
- [patterns/](patterns/) - 8个完整编程模式（事件、异步、物理等）
- [AI_DEVELOPER_GUIDE.md](AI_DEVELOPER_GUIDE.md) - API参考和系统架构导航

---

*This document should be included in AI system prompts for WasiCore development.*
