# 📚 WasiCore API Documentation

> The complete API documentation and development guide for the **WasiCore Game Framework**, designed for modern game development and AI-assisted programming.

## ⚡ Quick Start

### 🤖 Core Documentation for AI Developers

#### 📖 MUST READ (按顺序阅读)

**⭐ AI开发的唯一入口：[AI_QUICK_RULES.md](AI_QUICK_RULES.md)**

这个文档包含：
- 🚀 3分钟快速开始指南
- 🚨 5个关键规则（.NET 9.0、API使用、技术栈选择、GameMode配置、Scene配置）
- 📋 4步完整创建检查清单
- 🚨 常见错误速查
- 📚 延伸阅读链接

**然后阅读：**

1. **[🎯 Game Type Guide](GAME_TYPE_GUIDE.md)** - 技术栈选择详细指南（可选，如果规则3不够清楚）
   - 2D vs 3D决策树
   - Panel / Canvas / Shape / ECS对比
   - 游戏类型识别表

2. **[📖 示例代码](../src/)** - 对应游戏类型的完整示例（15分钟）
   - SuperMarioSimple.cs (Panel 2D)
   - FlappyBird.cs (Canvas 2D)
   - JumpJump.cs (Shape 3D)

#### 🔧 按需参考
- **[🎯 Game Type Guide](GAME_TYPE_GUIDE.md)** - 详细技术栈选择（如规则3不够清楚）NEW!
- **[🎨 UI Learning Path](UI_LEARNING_PATH.md)** - GameUI系统学习路径与文档索引 ⭐ UI主入口
- **[🖱️ UI Event Guide](guides/UI_EVENT_GUIDE.md)** - UI事件系统详细指南（拖放、Canvas等）NEW!
- **[🎨 AI Friendly UI API](guides/AI_FRIENDLY_UI_API.md)** - UI系统详细说明和流式API
- **[🚀 AI Developer Guide](AI_DEVELOPER_GUIDE.md)** - 总索引和API参考
- **[💡 Code Patterns](patterns/)** - 8个完整编程模式

### 📖 Essential Reading for Developers
- [🚀 5-Minute Quick Start](guides/QuickStart.md) - Get started with WasiCore immediately.
- [📋 Framework Overview](guides/FRAMEWORK_OVERVIEW.md) - Understand the design principles.
- [📖 Development Guides](guides/) - Entry point to all development documentation.

## 📁 Directory Structure

```
wasicore-api-docs/
├── 🤖 AI_DEVELOPER_GUIDE.md      # Condensed index guide for AI programming
├── 📋 AI_QUICK_RULES.md          # Essential rules for AI agents
├── 💡 patterns/                   # Complete code patterns
│   ├── Pattern00_ReferenceExisting.md # 🌟 ALWAYS check existing examples first!
│   ├── Pattern01_SystemInit.md   # Game system initialization
│   ├── Pattern02_DataDriven.md   # Data-driven object creation
│   ├── Pattern03_FluentUI.md     # Fluent UI building
│   ├── Pattern04_Events.md       # Event-driven game logic
│   ├── Pattern05_Async.md        # Async programming (WebAssembly-safe)
│   ├── Pattern06_SceneCreation.md # Scene creation with shapes
│   ├── Pattern07_ErrorHandling.md # Error handling and debugging
│   └── Pattern08_Physics.md      # Physics system (client-only)
├── 📖 guides/                  # Development Guides
│   ├── QuickStart.md             # Quick start tutorial
│   ├── ProjectStructure.md       # Project structure explanation
│   ├── AI_DEVELOPMENT_GUIDE.md   # AI Development Guide
│   ├── AI_FRIENDLY_UI_API.md     # AI-Friendly UI API
│   ├── Testing.md                # Testing guide
│   ├── CloudDataQuickStart.md    # Cloud data quick start
│   ├── EntityComponentDataPattern.md  # ECS pattern guide
│   ├── EntityVsActor.md          # Entity vs Actor concepts
│   ├── 💡 best-practices/         # Best Practices
│   │   ├── AsyncProgramming.md   # Best practices for asynchronous programming
│   │   ├── CloudDataBestPractices.md # Best practices for cloud data
│   │   └── CommonPitfalls.md     # Common pitfalls
│   ├── 🏗️ systems/                # System Architecture Documents (20 systems)
│   │   ├── GameDataSystem.md     # Data-Driven System
│   │   ├── UnitSystem.md         # Unit System
│   │   ├── AbilitySystem.md      # Ability System
│   │   ├── UIPropertySystem.md   # UI Property System
│   │   └── ...                   # Other 16 systems
│   └── FRAMEWORK_OVERVIEW.md     # Framework Architecture Overview
├── 📚 api-client-reference/        # Client API Reference (XML Documentation)
│   ├── GameCore.xml              # Game Core API
│   ├── GameUI.xml                # UI System API
│   └── ...                       # Other modules
└── 🖥️ api-server-reference/        # Server API Reference (XML Documentation)
    ├── GameCore.xml              # Game Core API
    ├── Events.xml                # Event System API
    └── ...                       # Other modules
```

## 🚀 Quick Navigation

### ⚡ Quick Start (Recommended)
- **[🤖 AI Developer Guide](AI_DEVELOPER_GUIDE.md) - One-stop AI programming guide** - ⭐ **Designed for AI programming, find APIs by intent**
- [🚀 Quick Start](guides/QuickStart.md) - Get up and running with WasiCore in 5 minutes
- [📋 Framework Overview](guides/FRAMEWORK_OVERVIEW.md) - Introduction to the overall architecture
- [📖 Project Structure](guides/ProjectStructure.md) - Understand how the project is organized
  
### 📚 API Reference
- [📱 Client API Reference](api-client-reference/) - XML documentation for client-side APIs
- [🖥️ Server API Reference](api-server-reference/) - XML documentation for server-side APIs

### 🎯 Core Documentation
- [🏗️ System Architecture](guides/systems/) - Detailed explanations of 20 systems
- [💡 Best Practices](guides/best-practices/) - Development experience and tips

### 🤖 AI Development Zone
- [🤖 AI Development Guide](guides/AI_DEVELOPMENT_GUIDE.md) - Guide to developing AI systems
- [🎨 UI Learning Path](UI_LEARNING_PATH.md) - GameUI系统学习路径与文档索引（⭐ UI主入口）
- [🎨 AI-Friendly UI API](guides/AI_FRIENDLY_UI_API.md) - Flow layout API design

### 🛠️ Development Resources
- [⚠️ FAQ & Common Pitfalls](guides/best-practices/CommonPitfalls.md) - Avoid common development traps


## 🎯 Documentation Features

### Client/Server Separation

This documentation system is specifically designed for the WasiCore framework's client/server separated architecture:

- **Client API** - Contains classes and interfaces relevant to the client.
- **Server API** - Contains classes and interfaces relevant to the server.

### Build Configuration Support

Supports all build configurations of the framework:

#### Client Configurations
- `Client-Debug` - Client debug build (default for documentation generation)
- `Client-Release` - Client release build

#### Server Configurations
- `Server-Debug` - Server debug build (default for documentation generation)
- `Server-Release` - Server release build

## 📊 Project Statistics

- **📁 20 System Documents**: Covers all core systems of WasiCore
- **🤖 1 Condensed AI Guide**: Quick index guide with links to patterns (< 4KB)
- **💡 9 Code Pattern Files**: Complete programming patterns with examples (Pattern 0 is crucial!)
- **📋 2 Rule Documents**: AI Quick Rules and main guide
- **📚 XML API Reference**: Complete C# standard documentation format
- **🎯 Optimized for AI**: Condensed main guide prevents token overflow, patterns loaded on demand

---

*WasiCore API Documentation - Designed for modern game development and AI programming | For questions or suggestions, please contact the development team.*
