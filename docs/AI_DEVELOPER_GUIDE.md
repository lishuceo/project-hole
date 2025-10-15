# 🤖 WasiCore AI Developer Guide (Condensed)

> **This is the main index for AI programming WasiCore games. For detailed patterns, see the linked documents.**
> 
> 🚨 **CRITICAL**: **ALWAYS** read [AI_QUICK_RULES.md](AI_QUICK_RULES.md) first!

## 📋 Quick Navigation

| What You Need | Where to Find It |
|--------------|------------------|
| **Essential Rules & Quick Start** | [AI_QUICK_RULES.md](AI_QUICK_RULES.md) ⭐ 唯一必读入口 |
| **Tech Stack Selection (详细)** | [GAME_TYPE_GUIDE.md](GAME_TYPE_GUIDE.md) 🎯 可选深入 |
| **UI Learning Path** | [UI_LEARNING_PATH.md](UI_LEARNING_PATH.md) 🎨 UI学习路径与文档索引 |
| **UI Event Guide** | [guides/UI_EVENT_GUIDE.md](guides/UI_EVENT_GUIDE.md) 🖱️ UI事件系统详细指南 |
| **UI API Details** | [guides/AI_FRIENDLY_UI_API.md](guides/AI_FRIENDLY_UI_API.md) 🎨 流式UI详细说明 |
| **Complete Code Patterns** | [patterns/](patterns/) directory |
| **API Documentation** | [api-client-reference/](api-client-reference/) & [api-server-reference/](api-server-reference/) |
| **System Guides** | [guides/](guides/) directory |

## 🚀 Recommended Reading Flow for AI

### For Creating a NEW Game
**Read in this order:**
1. 📋 [AI_QUICK_RULES.md](AI_QUICK_RULES.md) - All-in-one: rules + quick start + error reference (5 min)
2. 📖 Read the specific example code (chosen from Rule 3's table) (10-15 min)
3. 🎯 [GAME_TYPE_GUIDE.md](GAME_TYPE_GUIDE.md) - Optional: detailed tech stack guide (if Rule 3 unclear)

### For Fixing Compilation Errors
**Read in this order:**
1. 📋 [AI_QUICK_RULES.md](AI_QUICK_RULES.md) - Check "常见错误速查" section (1 min)
2. 🎨 [UI_LEARNING_PATH.md](UI_LEARNING_PATH.md) - If UI-related (学习路径和文档索引)
3. 🎨 [guides/AI_FRIENDLY_UI_API.md](guides/AI_FRIENDLY_UI_API.md) - If UI API error (optional)
4. 🔍 Search API docs: `grep -i "MethodName" api-*/**.xml`
5. 📖 [patterns/Pattern07_ErrorHandling.md](patterns/Pattern07_ErrorHandling.md) - Error patterns

### 🚨 **CRITICAL: Always Check API Documentation**
> **When encountering compilation errors or unsure about APIs:**
> - **NEVER guess** method names, parameters, or return types
> - **ALWAYS search** the XML documentation files
> - Use `grep` to find exact API signatures: `grep -i "MethodName" api-*/**.xml`

## 🎯 Core Concepts (Must Know)

### The Data-Driven Trinity
```
GameLink → GameData → GameObject
```
1. **GameLink**: Compile-time reference (e.g., `UnitLinks.MyHero`)
2. **GameData**: Runtime configuration (e.g., `new GameDataUnit(link)`)
3. **GameObject**: Actual game entity (e.g., `data.CreateUnit()`)

### Four Core Systems
- **🗄️ GameData**: Data-driven architecture
- **🎮 GameCore**: Entity-Component-System (ECS)
- **🎨 GameUI**: Modern fluent UI (`UI.Button().OnClick()`)
- **⚛️ Physics**: Physics engine integration

## 🔍 Quick API Reference

### Essential APIs
```csharp
// Game object creation
heroData.CreateUnit(player, position)
buildingData.CreateBuilding(player, position)

// UI (fluent API - PREFERRED)
// 💡 完整UI学习路径: UI_LEARNING_PATH.md
// 💡 UI事件系统: guides/UI_EVENT_GUIDE.md
UI.Button("Text").OnClick(handler)
UI.VStack(spacing, children...)

// Logging (NEVER use Console.WriteLine)
Game.Logger.LogInformation("Message {Param}", value)

// Async (NEVER use Task.Delay)
await Game.Delay(TimeSpan.FromSeconds(1))

// Physics (CLIENT only)
new PhysicsActor(player, shapeLink, scene, position, rotation)
```

### 🖱️ UI Event Quick Reference

#### 必需的命名空间
```csharp
using GameUI.Control.Struct;    // PointerEventArgs
using GameUI.TriggerEvent;      // EventGamePointerButton*
using GameUI.Device;            // DeviceInfo, InputManager
```

#### 坐标类型
```csharp
// UIPosition - 使用Left/Top（不是X/Y）
var uiPos = e.PointerPosition.Value;
float x = uiPos.Left;  // ✅
float y = uiPos.Top;   // ✅

// Vector2/3 - 使用X/Y/Z
var worldPos = new Vector3(10, 20, 30);
float x = worldPos.X;  // ✅
```

#### 事件对照表

| 需求 | 控件事件 | 全局事件 | 获取坐标 |
|-----|---------|---------|---------|
| 鼠标按下 | `OnPointerPressed` | `Trigger<EventGamePointerButtonDown>` | `e.PointerPosition.Value.Left/Top` |
| 鼠标松开 | `OnPointerReleased` | `Trigger<EventGamePointerButtonUp>` | `e.PointerPosition.Value.Left/Top` |
| 鼠标移动 | ❌ 不支持 | `InputManager.OnPointerButtonMove` | `e.PointerPosition.Value.Left/Top` |

**详细说明**: [guides/UI_EVENT_GUIDE.md](guides/UI_EVENT_GUIDE.md)

## 📚 Pattern Library Index

| Pattern | Description | Link |
|---------|-------------|------|
| **Reference Existing** 🌟 | **ALWAYS DO THIS FIRST!** Find similar examples | [Pattern00_ReferenceExisting.md](patterns/Pattern00_ReferenceExisting.md) |
| **System Registration** | How to create a game system with `IGameClass` | [Pattern01_SystemInit.md](patterns/Pattern01_SystemInit.md) |
| **Data-Driven Creation** | Creating units, buildings, items using GameData | [Pattern02_DataDriven.md](patterns/Pattern02_DataDriven.md) |
| **Fluent UI** | Modern UI creation with chained methods | [Pattern03_FluentUI.md](patterns/Pattern03_FluentUI.md) |
| **Event System** | Triggers and event handling | [Pattern04_Events.md](patterns/Pattern04_Events.md) |
| **Async Programming** | WebAssembly-safe async patterns | [Pattern05_Async.md](patterns/Pattern05_Async.md) |
| **Scene Creation** | Quick scene setup with shapes | [Pattern06_SceneCreation.md](patterns/Pattern06_SceneCreation.md) |
| **Error Handling** | Safe patterns and debugging | [Pattern07_ErrorHandling.md](patterns/Pattern07_ErrorHandling.md) |
| **Physics System** | Physics actors and components | [Pattern08_Physics.md](patterns/Pattern08_Physics.md) |

## ❌ Common Mistakes to Avoid

1. **Wrong .NET version** → Use .NET 9.0
2. **Guessing API names** → Search in api-*-reference/ XML files
3. **Creating from scratch** → Check src/ for similar examples first
4. **Missing game mode** → Define GameLink<GameDataGameMode> for new games
5. **Console.WriteLine** → Use Game.Logger
6. **Task.Delay** → Use Game.Delay
7. **Direct GameData creation** → Always use GameLink.Data
8. **Missing #if CLIENT** → Physics code is client-only

### 🔍 How to Find the Correct API
```bash
# Search for a method name
grep -i "CreateUnit" api-client-reference/*.xml

# Search for a class
grep -i "GameDataUnit" api-client-reference/*.xml

# Search with context
grep -B2 -A2 "CreateUnit" api-client-reference/*.xml
```

## 🎯 Quick Task Guide

| Task | Quick Solution |
|------|---------------|
| **Find correct API** | `grep -i "keyword" api-*-reference/*.xml` |
| **Fix compilation error** | Search exact error in XML docs, never guess |
| **Find similar game** | `grep -r "mechanic" src/` or check README files |
| **Create new game** | 1. Check src/ for similar example<br>2. Define GameMode in ScopeData<br>3. Register in GlobalConfig<br>4. Create game system with IGameClass |
| **Add UI** | Use fluent API: `UI.Button("Text").OnClick(...)`<br>参考 `UIFrameworkTest/` 流式UI最佳实践 |
| **Handle events** | `new Trigger<EventType>(handler).Register(Game.Instance)` |
| **Add physics** | Wrap in `#if CLIENT`, use `PhysicsActor` |
| **Debug issues** | Check Game.Logger output, use try-catch |

---

*For complete code examples, navigate to the [patterns/](patterns/) directory.*
