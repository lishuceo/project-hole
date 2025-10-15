# 🎮 WasiCore游戏类型选择指南

> **AI开发游戏时的第一步：选择正确的技术栈**

## 🎯 核心原则

**在开始编码前，必须先确定游戏类型和实现方式！**

错误的技术栈选择会导致：
- ❌ 游戏角色看不见（3D相机问题）
- ❌ 频繁的API错误（使用了错误的API）
- ❌ 性能问题（用复杂系统实现简单游戏）
- ❌ 浪费大量开发时间

---

## 📊 决策流程图

```
用户请求创建游戏
    ↓
第一步：判断游戏维度（2D or 3D）
    ↓
    ├─── 2D游戏 ────────────────────────┐
    │                                   ↓
    │    第二步：判断渲染复杂度          │
    │        ↓                         │
    │        ├─ 简单形状、网格          │
    │        │  → Panel-based          │
    │        │     Game2048            │
    │        │     SuperMarioSimple    │
    │        │                         │
    │        └─ 复杂渲染、粒子效果      │
    │           → Canvas-based         │
    │              FlappyBird          │
    │                                   │
    └─── 3D游戏 ────────────────────────┘
                 ↓
         第二步：判断系统复杂度
             ↓
             ├─ 简单原型、形状展示
             │  → Shape-based
             │     JumpJump
             │     PrimitiveShapeTest
             │
             └─ 完整游戏系统、单位、技能
                → ECS-based
                   VampireSurvivors3D
                   ARPGTemplate
                   TowerDefenseGame
```

---

## 🔍 游戏类型识别表

### 根据用户输入识别游戏类型

| 用户可能说的词汇 | 游戏类型 | 推荐技术栈 | 参考示例 |
|----------------|---------|-----------|---------|
| 超级玛丽、马里奥、平台跳跃、横版 | 2D横版动作 | **Panel-based** | SuperMarioSimple |
| Flappy Bird、直升机、躲避 | 2D飞行 | Canvas-based | FlappyBird |
| 俄罗斯方块、消消乐、三消 | 2D拼图 | Panel-based | Game2048 |
| 2048、合成大西瓜 | 2D网格 | Panel-based | Game2048 |
| 五子棋、象棋、围棋 | 2D棋盘 | Panel-based | Gomoku |
| 贪吃蛇、吃豆人 | 2D迷宫 | Panel-based | (待创建) |
| 跳一跳、神庙逃亡、跑酷 | 3D跑酷 | Shape-based | JumpJump |
| 吸血鬼幸存者、弹幕 | 3D生存 | ECS-based | VampireSurvivors3D |
| 塔防、守卫 | 3D塔防 | ECS-based | TowerDefenseGame |
| ARPG、剑客、地牢 | 3D角色扮演 | ECS-based | ARPGTemplate |
| 第一人称、FPS | 3D射击 | ECS-based | (待创建) |

---

## 📖 详细技术栈说明

### 1️⃣ Panel-based 2D (推荐用于大多数2D游戏)

#### 适用场景
- ✅ 简单的2D形状（方块、圆形）
- ✅ 网格类游戏（棋盘、拼图）
- ✅ 回合制游戏
- ✅ 简单的平台跳跃游戏
- ✅ UI密集型游戏

#### 核心API
```csharp
// 创建游戏区域
var gameArea = new Panel {
    Width = 800,
    Height = 600,
    Background = new SolidColorBrush(Color.SkyBlue)
};

// 创建游戏对象（角色、敌人、道具）
var character = new Panel {
    Width = 32,
    Height = 32,
    Position = new UIPosition(x, y),
    PositionType = UIPositionType.Absolute,
    Background = new SolidColorBrush(Color.Red)
};
gameArea.AddChild(character);

// 添加文字/图标
var label = new Label {
    Text = "M",
    Position = new UIPosition(8, 8),
    PositionType = UIPositionType.Absolute
};
character.AddChild(label);

// 更新位置（每帧）
character.Position = new UIPosition(newX, newY);
```

#### 优点
- ✅ 简单直观，易于调试
- ✅ 不需要相机设置
- ✅ UI工具可以直接预览
- ✅ 碰撞检测简单（矩形对矩形）

#### 缺点
- ❌ 渲染能力有限（无渐变、无粒子效果）
- ❌ 大量对象时性能可能不如Canvas
- ❌ 旋转、缩放等变换支持有限

#### 示例代码
```csharp
// 参考：src/SuperMarioGame/SuperMarioSimple.cs
// 参考：src/Game2048/Game2048.cs
// 参考：src/Gomoku/GomokuGame.cs
```

---

### 2️⃣ Canvas-based 2D

#### 适用场景
- ✅ 需要自定义绘制（渐变、阴影）
- ✅ 粒子效果、特效
- ✅ 大量动态对象
- ✅ 复杂的动画
- ✅ 需要高性能渲染

#### 核心API
```csharp
// 创建Canvas
var gameCanvas = new Canvas {
    Width = 800,
    Height = 600
};

// 每帧绘制循环
private void DrawGame()
{
    // 1. 清空画布
    gameCanvas.ResetState();
    
    // 2. 设置绘制样式
    gameCanvas.FillPaint = new SolidPaint(Color.Red);
    gameCanvas.StrokePaint = new SolidPaint(Color.Black);
    gameCanvas.StrokeWidth = 2f;
    
    // 3. 绘制形状
    gameCanvas.FillRectangle(x, y, width, height);
    gameCanvas.DrawLine(x1, y1, x2, y2);
    gameCanvas.FillCircle(centerX, centerY, radius);
    gameCanvas.FillTriangle(x1, y1, x2, y2, x3, y3);
    
    // 4. 绘制文字
    gameCanvas.DrawText("Hello", x, y, fontSize);
    
    // 5. 变换（可选）
    gameCanvas.SaveState();
    gameCanvas.Translate(dx, dy);
    gameCanvas.Rotate(angle);
    gameCanvas.FillCircle(0, 0, 10); // 在变换后的坐标系中绘制
    gameCanvas.RestoreState();
}
```

#### 优点
- ✅ 完全的2D渲染自由度
- ✅ 高性能（直接绘制，无UI树遍历）
- ✅ 支持复杂视觉效果
- ✅ 适合大量粒子和动画

#### 缺点
- ❌ 复杂度高，需要手动管理绘制
- ❌ 不能使用UI控件（Button、Label）混合
- ❌ 调试困难（看不到可视化的UI树）

#### 示例代码
```csharp
// 参考：src/FlappyBirdGame/FlappyBird.cs (完整Canvas使用示例)
// DrawGame(), DrawBird(), DrawPipes()等方法
```

#### ⚠️ Canvas常见错误
```csharp
// ❌ 错误1：尝试操作Children
gameCanvas.Children.Clear();  // 编译错误！
gameCanvas.Children.Add(new Panel());  // 编译错误！

// ❌ 错误2：忘记每帧ResetState
DrawGame() {
    // 忘记清空，会叠加绘制！
    gameCanvas.FillRectangle(...);
}

// ✅ 正确
DrawGame() {
    gameCanvas.ResetState();  // 必须先清空
    gameCanvas.FillRectangle(...);
}
```

---

### 3️⃣ Shape-based 3D

#### 适用场景
- ✅ 3D原型快速开发
- ✅ 简单的3D形状展示
- ✅ 不需要复杂游戏系统
- ✅ 物理演示、测试

#### 核心API
```csharp
// 创建3D形状
var actor = AIShapeFactory.CreateShape(
    PrimitiveShape.Cube,  // Cube, Sphere, Capsule, Cone, Cylinder, etc.
    new ScenePoint(new Vector3(x, y, z), Game.LocalScene),
    new Vector3(scaleX, scaleY, scaleZ),
    null  // scope
);

// 设置颜色
if (actor is IActorColorizable tintable)
{
    tintable.InitializeTintColorAggregators();
    tintable.SetTintColor(
        new HdrColor(Color.Red),
        GameCore.ActorSystem.Enum.TintColorType.Override,
        "my_color"
    );
}

// 更新位置
actor.Position = new ScenePoint(new Vector3(newX, newY, newZ), Game.LocalScene);

// 设置相机（必须！）
ScreenViewport.Primary.Camera.SetPosition(
    new Vector3(cameraX, cameraY, cameraZ),
    TimeSpan.FromSeconds(0.1)
);
ScreenViewport.Primary.Camera.SetRotation(
    new GameCore.CameraSystem.Struct.CameraRotation(yaw, pitch, roll),
    TimeSpan.FromSeconds(0.1)
);
```

#### 优点
- ✅ 快速创建3D原型
- ✅ 不需要复杂的数据定义
- ✅ 适合演示和测试

#### 缺点
- ❌ 没有游戏系统（单位、技能、buff）
- ❌ 相机设置复杂，容易出错
- ❌ 碰撞检测需要手动实现

#### 示例代码
```csharp
// 参考：src/JumpJumpGame/JumpJump.cs
// 参考：src/PrimitiveShapeTest/PrimitiveShapeTestClient.cs
```

#### ⚠️ 3D常见错误
```csharp
// ❌ 错误1：忘记设置相机（角色看不见！）
var actor = AIShapeFactory.CreateShape(...);
// 忘记设置相机 → 看不见角色！

// ✅ 正确：必须设置相机
SetupCamera();

// ❌ 错误2：Y和Z轴混淆
new Vector3(x, y, z)  // WasiCore: X水平, Y深度, Z高度！
// 很多人以为Y是高度，但实际上Z才是高度

// ❌ 错误3：相机角度不对
CameraRotation(0, 0, 0)  // 可能看不见
// 需要合适的俯视角，如(-90, -70, 0)
```

---

### 4️⃣ ECS-based 3D (完整游戏系统)

#### 适用场景
- ✅ 完整的游戏（单位、技能、物品）
- ✅ ARPG、塔防、生存类游戏
- ✅ 需要服务器验证的游戏逻辑
- ✅ 复杂的游戏机制

#### 核心API
```csharp
// 定义单位数据
_ = new GameDataUnit(UnitLinks.MyHero)
{
    Name = "Hero",
    Properties = new() {
        { UnitProperty.LifeMax, 1000 },
        { UnitProperty.MoveSpeed, 300 }
    }
};

// 创建单位实例
var unit = UnitLinks.MyHero.Data.CreateUnit(player, position);

// 使用技能系统
var ability = AbilityLinks.MySkill.Data;
unit.CastAbility(ability, target);

// Buff系统
unit.AddBuff(BuffLinks.SpeedBuff.Data, duration);
```

#### 优点
- ✅ 完整的游戏系统
- ✅ 数据驱动，易于平衡调整
- ✅ 服务器客户端分离
- ✅ 支持网络同步

#### 缺点
- ❌ 学习曲线陡峭
- ❌ 需要大量GameData定义
- ❌ 对简单游戏来说过度设计

#### 示例代码
```csharp
// 参考：src/ARPGTemplate/ (最完整的示例)
// 参考：src/VampireSurvivors3D/
// 参考：src/TowerDefenseGame/
```

---

## 🧭 AI决策助手

### 当用户说...时，AI应该...

#### "做一个超级玛丽"
```python
识别: "超级玛丽" → 2D横版平台跳跃
选择: Panel-based 2D
参考: SuperMarioSimple.cs
理由: 简单的方块和平台，不需要复杂渲染
```

#### "做一个Flappy Bird"
```python
识别: "Flappy Bird" → 2D飞行躲避
选择: Canvas-based 2D (或Panel-based简化版)
参考: FlappyBird.cs
理由: 需要自定义绘制（管道、粒子效果）
```

#### "做一个俄罗斯方块"
```python
识别: "俄罗斯方块" → 2D网格拼图
选择: Panel-based 2D
参考: Game2048.cs
理由: 网格系统，用Panel最合适
```

#### "做一个跳一跳"
```python
识别: "跳一跳" → 3D平台跳跃
选择: Shape-based 3D
参考: JumpJump.cs
理由: 3D场景，简单形状即可
```

#### "做一个吸血鬼幸存者"
```python
识别: "吸血鬼幸存者" → 3D生存游戏
选择: ECS-based 3D
参考: VampireSurvivors3D/
理由: 需要完整的单位、技能、升级系统
```

#### "做一个塔防游戏"
```python
识别: "塔防" → 3D策略游戏
选择: ECS-based 3D
参考: TowerDefenseGame/
理由: 需要建筑、资源、波次系统
```

---

## 🔧 技术栈对比表

| 特性 | Panel 2D | Canvas 2D | Shape 3D | ECS 3D |
|------|---------|-----------|----------|--------|
| **学习难度** | ⭐ 最简单 | ⭐⭐ 中等 | ⭐⭐⭐ 较难 | ⭐⭐⭐⭐⭐ 最难 |
| **开发速度** | ⚡⚡⚡ 最快 | ⚡⚡ 快 | ⚡ 慢 | ⏱️ 最慢 |
| **渲染能力** | 基础 | 强大 | 3D | 3D |
| **相机设置** | ❌ 不需要 | ❌ 不需要 | ✅ 必须 | ✅ 必须 |
| **碰撞检测** | 简单矩形 | 手动实现 | 手动实现 | 内置系统 |
| **网络同步** | 手动 | 手动 | 手动 | 自动 |
| **适合游戏** | 2D休闲 | 2D动作 | 3D原型 | 3D大作 |

---

## 📝 AI工作流示例

### 完整的开发流程

```markdown
1. **读取用户需求**
   用户："帮我做一个超级玛丽"

2. **识别游戏类型**（查阅本文档）
   "超级玛丽" → 2D横版平台跳跃 → Panel-based 2D

3. **选择参考示例**（查阅快速参考表）
   推荐示例：SuperMarioSimple.cs
   备选示例：Game2048.cs（Panel用法）

4. **读取参考代码**
   read_file("src/SuperMarioGame/SuperMarioSimple.cs")
   read_file("src/Game2048/Game2048.cs")

5. **理解核心结构**
   - Panel作为游戏对象
   - Position更新实现移动
   - IThinker.Think()实现游戏循环

6. **复制并修改**
   - 复制Initialize(), CreateUI(), Think()结构
   - 修改游戏逻辑：平台、跳跃、收集
   - 保持框架代码不变

7. **遵循配置规则**
   - 在ScopeData.cs中定义GameMode和Scene
   - 在GlobalConfig.cs中注册
   - 创建专用Scene（PlacedPlayerObjects = []）
```

---

## ⚠️ 常见陷阱警告

### 陷阱1：混用不同技术栈
```csharp
// ❌ 错误：在Panel游戏中使用3D API
var panel = new Panel { ... };
AIShapeFactory.CreateShape(...);  // 混用！会导致混乱
```

### 陷阱2：选错示例导致架构不匹配
```csharp
// ❌ 错误：做2D游戏却参考了3D示例
// 用户："做超级玛丽"
// AI错误地参考：JumpJump.cs（3D游戏）
// 结果：角色看不见，因为相机角度问题
```

### 陷阱3：过度设计
```csharp
// ❌ 错误：简单2D游戏使用ECS系统
// 用户："做个五子棋"
// AI错误地使用：GameDataUnit, CreateUnit, etc.
// 结果：过度复杂，应该用Panel即可
```

---

## 🎯 快速决策工具

### AI应该问自己的3个问题

1. **这是2D游戏还是3D游戏？**
   - 关键词判断
   - 用户明确说明
   - 游戏原型推测

2. **需要多复杂的渲染？**
   - 简单方块 → Panel
   - 复杂效果 → Canvas (2D) 或 Shape/ECS (3D)

3. **需要完整的游戏系统吗？**
   - 只是原型/简单游戏 → Panel/Canvas/Shape
   - 完整游戏 → ECS

---

## 📚 延伸阅读

按顺序阅读：

1. [AI_QUICK_RULES.md](AI_QUICK_RULES.md) - 必读基础规则
2. **[GAME_TYPE_GUIDE.md](GAME_TYPE_GUIDE.md)** - 本文档（技术栈选择）
3. [IMPLICIT_KNOWLEDGE.md](IMPLICIT_KNOWLEDGE.md) - 隐式知识避坑
4. [AI_DEVELOPER_GUIDE.md](AI_DEVELOPER_GUIDE.md) - 开发指南
5. [patterns/](patterns/) - 编程模式

---

*这个指南帮助AI在第一步就选择正确的技术栈，避免90%的开发错误。*
