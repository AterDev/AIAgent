# AG-UI 集成更新说明

> **更新日期**: 2025-01-27  
> **更新内容**: 使用官方 Microsoft.Agents.AI.Hosting.AGUI.AspNetCore 包替代自定义实现

---

## 变更说明

根据项目维护者 @niltor 的反馈，我们已将 AG-UI 集成方案更新为使用 **Microsoft 官方包**：`Microsoft.Agents.AI.Hosting.AGUI.AspNetCore`。

### 主要变更

#### 1. 添加官方 NuGet 包

**文件**: `Directory.Packages.props`

```xml
<PackageVersion Include="Microsoft.Agents.AI.Hosting.AGUI.AspNetCore" Version="1.0.0-preview.260121.1" />
<PackageVersion Include="Microsoft.Extensions.AI.Abstractions" Version="1.0.0-preview.1.25071.6" />
```

#### 2. 创建 AG-UI 扩展方法

**文件**: `src/Services/ApiService/Extensions/AgUiExtensions.cs`

使用官方 API 提供简洁的扩展方法：

```csharp
public static class AgUiExtensions
{
    // 添加 AG-UI 服务
    public static IServiceCollection AddAGUIServices(this IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddLogging();
        services.AddAGUI(); // 官方 AG-UI 服务
        return services;
    }

    // 映射 AG-UI 端点
    public static WebApplication UseAGUIEndpoints(this WebApplication app)
    {
        app.MapAGUIEndpoints(); // 自动创建 /agents 端点
        return app;
    }
}
```

#### 3. 更新 Program.cs

**文件**: `src/Services/ApiService/Program.cs`

```csharp
// 添加 AG-UI 服务
builder.Services.AddAGUIServices();

// ... 其他配置 ...

// 启用 AG-UI 端点
app.UseAGUIEndpoints();
```

---

## 官方 AG-UI 功能

使用官方包后，您将获得以下开箱即用的功能：

### 1. 自动端点映射

官方包会自动创建以下端点：

- `GET /agents` - 列出所有可用的 Agent
- `POST /agents/{agentId}/invoke` - 调用指定 Agent
- `GET /agents/{agentId}/stream` - 流式调用 Agent（SSE）

### 2. AG-UI 协议支持

完整支持 AG-UI 协议标准：

- ✅ 实时流式响应（Server-Sent Events）
- ✅ 标准化消息格式
- ✅ 工具调用可视化
- ✅ 会话状态管理
- ✅ 前端库集成（如 CopilotKit）

### 3. 与 Microsoft Agent Framework 集成

无缝集成 Microsoft Agent Framework 生态：

- ✅ 使用 `IHostApplicationBuilder.AddAIAgent()` 注册 Agent
- ✅ 支持 `IChatClient` 接口（Azure OpenAI、OpenAI 等）
- ✅ 自动处理工具调用和多轮对话
- ✅ 内置 OpenTelemetry 追踪支持

---

## 快速开始

### 1. 配置 Chat Client

要使用 AG-UI，需要先配置一个 `IChatClient`（例如 Azure OpenAI）：

```csharp
// 在 Program.cs 或 appsettings.json 中配置
string endpoint = builder.Configuration["AZURE_OPENAI_ENDPOINT"];
string deploymentName = builder.Configuration["AZURE_OPENAI_DEPLOYMENT_NAME"];

var chatClient = new AzureOpenAIClient(
    new Uri(endpoint), 
    new DefaultAzureCredential()
).GetChatClient(deploymentName).AsIChatClient();

// 注册为服务
builder.Services.AddSingleton<IChatClient>(chatClient);
```

### 2. 注册 AI Agent

```csharp
builder.AddAIAgent(
    "assistant",
    instructions: "You are a helpful AI assistant.",
    description: "A general-purpose AI assistant.",
    chatClientServiceKey: "chat-model"
);
```

### 3. 测试 AG-UI 端点

启动应用后，访问：

```bash
# 列出所有 Agent
curl http://localhost:5000/agents

# 调用 Agent
curl -X POST http://localhost:5000/agents/assistant/invoke \
  -H "Content-Type: application/json" \
  -d '{"message": "Hello, how are you?"}'

# 流式调用（SSE）
curl http://localhost:5000/agents/assistant/stream?message=Hello
```

---

## 前端集成

### 使用 CopilotKit

AG-UI 官方支持 CopilotKit 前端库：

```typescript
import { CopilotKit } from "@copilotkit/react-core";

function App() {
  return (
    <CopilotKit agentUrl="http://localhost:5000/agents/assistant">
      {/* Your app components */}
    </CopilotKit>
  );
}
```

### 自定义前端

也可以直接使用 EventSource API 连接 SSE 端点：

```javascript
const eventSource = new EventSource(
  'http://localhost:5000/agents/assistant/stream?message=Hello'
);

eventSource.onmessage = (event) => {
  const data = JSON.parse(event.data);
  console.log('Agent response:', data);
};
```

---

## 与自定义实现的对比

| 特性 | 自定义实现 | 官方包 |
|------|----------|--------|
| WebSocket 支持 | ✅ | ❌ (使用 SSE) |
| SSE 支持 | ❌ | ✅ |
| 标准化端点 | ❌ | ✅ |
| 前端库集成 | 需要自定义 | ✅ 开箱即用 |
| 维护成本 | 高 | 低（官方维护） |
| 协议兼容性 | 自定义 | ✅ AG-UI 标准 |
| 文档支持 | 需要自行维护 | ✅ 官方文档 |

---

## 后续开发建议

### 1. 集成真实的 LLM 提供商

目前需要配置实际的 `IChatClient`，建议：

```csharp
// 选项 1: Azure OpenAI
builder.Services.AddSingleton<IChatClient>(sp => 
{
    var config = sp.GetRequiredService<IConfiguration>();
    var endpoint = config["AzureOpenAI:Endpoint"];
    var deploymentName = config["AzureOpenAI:DeploymentName"];
    
    return new AzureOpenAIClient(
        new Uri(endpoint), 
        new DefaultAzureCredential()
    ).GetChatClient(deploymentName).AsIChatClient();
});

// 选项 2: OpenAI
builder.Services.AddSingleton<IChatClient>(sp => 
{
    var config = sp.GetRequiredService<IConfiguration>();
    var apiKey = config["OpenAI:ApiKey"];
    
    return new OpenAIClient(apiKey)
        .GetChatClient("gpt-4")
        .AsIChatClient();
});
```

### 2. 注册实际的 Agent

从数据库加载 Agent 配置并注册：

```csharp
// 在启动时加载 Agent 配置
var agentConfigs = await LoadAgentConfigsFromDatabase();

foreach (var config in agentConfigs)
{
    builder.AddAIAgent(
        config.Name,
        instructions: config.SystemPrompt,
        description: config.Description,
        chatClientServiceKey: "chat-model"
    );
}
```

### 3. 实现工具调用

为 Agent 添加工具能力：

```csharp
// 定义工具
public class SearchTool
{
    [Description("Search the knowledge base")]
    public async Task<string> Search(
        [Description("Search query")] string query)
    {
        // 实现知识库搜索逻辑
        return "Search results...";
    }
}

// 注册工具
builder.Services.AddSingleton<SearchTool>();
builder.AddAIAgent("assistant")
    .WithTools<SearchTool>();
```

### 4. 添加前端 UI

可以选择：

- **CopilotKit**: 快速构建聊天 UI
- **自定义 Angular 组件**: 集成到现有的 Angular 应用
- **第三方 AG-UI 客户端**: 任何支持 AG-UI 协议的客户端

---

## 迁移说明

如果之前使用了自定义实现，可以：

### 保留自定义实现

如果有特殊需求（如 WebSocket），可以同时保留自定义实现：

```csharp
// 官方 AG-UI（SSE）
builder.Services.AddAGUIServices();

// 自定义 WebSocket 实现
builder.Services.AddSingleton<AgUiCommunicationService>();
builder.Services.AddScoped<IStreamingAgentExecutor, StreamingAgentExecutor>();

// 映射两种端点
app.UseAGUIEndpoints();        // 官方: /agents
app.MapControllers();           // 自定义: /api/agui
```

### 完全迁移到官方实现

1. 移除自定义的 `AgUiCommunicationService`、`StreamingAgentExecutor`、`AgUiController`
2. 使用官方 `AddAGUI()` 和 `MapAGUIEndpoints()`
3. 更新前端代码连接到 `/agents` 端点

---

## 参考资源

### 官方文档

1. **Getting Started**: https://learn.microsoft.com/en-us/agent-framework/integrations/ag-ui/getting-started
2. **AG-UI Protocol**: https://learn.microsoft.com/en-us/agent-framework/integrations/ag-ui/
3. **Hosting Guide**: https://learn.microsoft.com/en-us/agent-framework/user-guide/hosting/

### GitHub 资源

- **Agent Framework**: https://github.com/microsoft/agent-framework
- **AGUI.AspNetCore Source**: https://github.com/microsoft/agent-framework/tree/main/dotnet/src/Microsoft.Agents.AI.Hosting.AGUI.AspNetCore

### 社区资源

- **CopilotKit Blog**: https://www.copilotkit.ai/blog/build-a-frontend-for-your-microsoft-agent-framework-agents-with-ag-ui

---

## 总结

使用官方 `Microsoft.Agents.AI.Hosting.AGUI.AspNetCore` 包的优势：

✅ **标准化**: 遵循 AG-UI 协议标准  
✅ **维护成本低**: 由 Microsoft 官方维护和更新  
✅ **生态集成**: 与 Agent Framework 和前端库无缝集成  
✅ **功能完整**: 开箱即用的流式响应、工具调用等功能  
✅ **文档完善**: 官方文档和示例丰富  

下一步建议优先配置真实的 `IChatClient` 并注册实际的 Agent，以便充分利用 AG-UI 的强大功能。

---

**变更记录**:
- 2025-01-27: 从自定义实现迁移到官方 Microsoft.Agents.AI.Hosting.AGUI.AspNetCore 包
