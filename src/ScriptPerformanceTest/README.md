# 脚本执行效率测试模式

## 概述

ScriptPerformanceTest 是一个专门用于测试脚本环境执行效率的游戏模式。该模式无需场景和默认场景，仅提供UI界面进行性能测试。

## 功能特性

### 🎯 测试范围
- **服务端脚本测试**: 测试服务端脚本执行环境的性能
- **客户端脚本测试**: 测试客户端脚本执行环境的性能

### 🧪 测试项目
每个测试环境提供三种测试：
1. **测试加法X次**: 纯计算性能测试
2. **测试脚本调用宿主X次**: 脚本->宿主调用性能
3. **测试宿主调用脚本X次**: 宿主->脚本调用性能

### 🎨 UI界面
- **流式布局**: 使用现代化的流式UI布局，界面清晰直观
- **动态输入**: 可自定义测试次数（默认10000次）
- **实时结果**: 显示详细的性能测试结果
- **分类展示**: 服务端和客户端测试结果分别展示

## 使用方法

### 1. 启动测试模式

在 `GlobalConfig.cs` 中设置测试模式：
```csharp
GameDataGlobalConfig.TestGameMode = ScopeData.GameMode.ScriptPerformanceTest;
```

或者在运行时选择 "ScriptPerformanceTest" 模式。

### 2. 界面操作

1. **设置测试次数**: 在输入框中输入要测试的次数（默认10000）
2. **选择测试环境**: 点击服务端或客户端区域的测试按钮
3. **选择测试类型**: 
   - 加法测试：纯计算性能
   - 脚本调用宿主：脚本->宿主接口调用性能
   - 宿主调用脚本：宿主->脚本函数调用性能
4. **查看结果**: 测试完成后查看详细的性能数据

### 3. 结果解读

测试结果包含以下信息：
- **执行次数**: 实际执行的测试次数
- **总耗时**: 总共花费的时间（毫秒）
- **平均耗时**: 单次操作的平均耗时（纳秒）
- **每秒执行**: 每秒能执行的操作数量

## 开发者定制

### 实现自定义测试逻辑

系统现在使用TypedMessage进行客户端-服务端通信。服务端测试在服务端执行，客户端测试在客户端执行：

#### 服务端测试实现
服务端测试逻辑在 `ScriptPerformanceTestServer.cs` 中实现：

```csharp
// 在 ScriptPerformanceTestServer.cs 中修改以下方法：
private static async Task ExecuteAdditionTest(int count, ScriptPerformanceTestResult result)
private static async Task ExecuteScriptToHostCallTest(int count, ScriptPerformanceTestResult result)  
private static async Task ExecuteHostToScriptCallTest(int count, ScriptPerformanceTestResult result)
```

#### 客户端测试实现
客户端测试逻辑在 `ScriptPerformanceTestMode.cs` 中实现：

```csharp
// 在 ScriptPerformanceTestMode.cs 中修改以下方法：
private void RunClientAdditionTest()     // 客户端加法测试
private void RunClientScriptToHostTest() // 客户端脚本调用宿主测试
private void RunClientHostToScriptTest() // 客户端宿主调用脚本测试
```

### 服务端测试实现示例

```csharp
private static async Task ExecuteAdditionTest(int count, ScriptPerformanceTestResult result)
{
    // TODO: 用户可以在这里实现实际的脚本加法测试逻辑
    // 这里提供一个基础的示例，用户应该替换为实际的脚本引擎调用
    
    double total = 0;
    for (int i = 0; i < count; i++)
    {
        // 替换为实际的脚本引擎加法调用
        // total += ScriptEngine.Execute($"return {i} + {i * 1.5}");
        total += i * 1.5; // 占位符实现
    }
    
    result.AdditionalInfo["FinalResult"] = total;
    result.AdditionalInfo["TestDescription"] = "Script engine addition test";
    
    await Game.Delay(TimeSpan.FromMilliseconds(1));
}
```

### TypedMessage通信流程

1. **客户端**: 用户点击服务端测试按钮
2. **客户端**: 创建 `ScriptPerformanceTestRequest` 消息发送到服务端
3. **服务端**: 接收请求，执行实际的脚本性能测试
4. **服务端**: 完成测试后，发送 `ScriptPerformanceTestResult` 消息回客户端
5. **客户端**: 接收结果并在UI中显示详细的性能数据

## 技术实现

### 架构设计
- **条件编译**: 使用 `#if CLIENT` 和 `#if SERVER` 确保代码只在对应端编译
- **TypedMessage通信**: 使用强类型消息进行客户端-服务端通信
- **单例模式**: 使用单例模式管理游戏模式实例
- **事件驱动**: 通过游戏系统事件进行初始化
- **流式UI**: 使用WasiCore的流式UI API构建界面

### 通信机制
- **消息定义**: `ScriptPerformanceTestRequest` 和 `ScriptPerformanceTestResult`
- **异步处理**: 所有网络通信都是异步的，不阻塞UI
- **超时处理**: 30秒超时机制，防止请求挂起
- **错误处理**: 完整的错误处理和用户反馈

### 性能考虑
- **高精度计时**: 使用 `Stopwatch` 进行高精度性能测量
- **服务端执行**: 服务端测试在服务端执行，避免网络延迟影响
- **内存友好**: 避免在测试循环中分配内存
- **线程安全**: 确保UI更新在主线程进行

### 扩展性
- **模块化设计**: 测试方法独立，易于扩展新的测试类型
- **配置化**: 测试参数可通过UI动态配置
- **结果标准化**: 统一的结果格式，包含详细的性能指标
- **双端支持**: 同时支持客户端和服务端性能测试

## 注意事项

1. **测试环境**: 确保在相同的环境条件下进行测试以获得可比较的结果
2. **测试次数**: 建议测试次数不少于1000次以获得稳定的性能数据
3. **资源占用**: 大量测试可能占用较多CPU资源，注意系统负载
4. **结果分析**: 关注平均耗时（纳秒）和每秒执行次数，而不仅仅是总耗时

## 未来扩展

可以考虑添加以下功能：
- **批量测试**: 支持多种测试的批量执行
- **结果导出**: 支持将测试结果导出为CSV或JSON格式
- **对比分析**: 支持多次测试结果的对比分析
- **内存测试**: 添加内存分配和垃圾回收的性能测试
- **异步测试**: 添加异步操作的性能测试

---

**开发状态**: ✅ 基础框架完成，等待脚本测试逻辑实现
