---
name: maf
description: "Microsoft Agent Framework 1.1 (C#) 开发规范与集成要点。USE FOR: ChatClientAgent、AIFunctionFactory、IChatClient tools、AgentThread/Conversation、ResponseFormat、Handoff/Workflow、MCP tool、Memory/ContextProvider、Observability、Foundry Local OpenAI 兼容接入。"
---

# Microsoft Agent Framework 1.1 Skill

## 何时使用

任务涉及以下内容时必读：
- 使用 `ChatClientAgent` 构建基于 OpenAI 兼容 provider（含 Foundry Local、DeepSeek、Azure OpenAI）的智能体
- 用 `AIFunctionFactory` 将业务服务（RAG、MCP、SQL、HTTP）包成 `AITool`
- Workflow：`Microsoft.Agents.AI.Workflows` 的 executor/edge/handoff/human-in-the-loop/checkpointing
- 结构化输出（`ChatResponseFormatJson`）、多模态（`DataContent`/`UriContent`）、Memory（`AgentThread` + Summary）
- Observability（`ActivitySource`、OpenTelemetry GenAI 语义）
- MCP：`ModelContextProtocol.Client` stdio/http/sse 接入

## 关键 NuGet 包（本仓库已通过中央包管理引入）

| 用途 | 包 | 版本 |
|---|---|---|
| Agent 抽象 | `Microsoft.Agents.AI` | 1.1.0 |
| OpenAI/兼容 provider | `Microsoft.Agents.AI.OpenAI` | 1.1.0 |
| 图式工作流 | `Microsoft.Agents.AI.Workflows` | 1.1.0 |
| 底层 Chat/Embedding 抽象 | `Microsoft.Extensions.AI.Abstractions` | 10.5.0 |
| OpenAI 客户端 | `OpenAI` | 2.10.0 |
| MCP 客户端 | `ModelContextProtocol` | 1.2.0 |

## 核心 API 速查

### 1. 用 IChatClient 构建 Agent

```csharp
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

var openAIClient = new OpenAIClient(
    new ApiKeyCredential(apiKey),
    new OpenAIClientOptions { Endpoint = new Uri(baseUrl) });

IChatClient chatClient = openAIClient.GetChatClient(modelId).AsIChatClient();
var agent = new ChatClientAgent(
    chatClient,
    instructions: "你是一位专业翻译助手。",
    tools: [ragTool, mcpTool]); // 可选

AgentThread thread = agent.GetNewThread();
var response = await agent.RunAsync("翻译这段话...", thread);
```

### 2. 函数工具（Skill）

```csharp
AITool ragTool = AIFunctionFactory.Create(
    async (string query, int topK, CancellationToken ct) =>
    {
        var result = await ragService.QueryAsync(new RagQueryRequest
        {
            Query = query,
            TopK = topK
        }, ct);
        return result;
    },
    name: "query_knowledge_base",
    description: "查询知识库并返回最相关的文档片段");
```

对只声明、不在本地执行的工具（由 provider 代执行，例如 OpenAI MCP Tool），使用
`AIFunctionFactory.CreateDeclaration(name, description, jsonSchema)`。

### 3. 结构化输出

```csharp
var options = new ChatOptions
{
    ResponseFormat = ChatResponseFormat.ForJsonSchema(
        jsonSchema: schemaJsonElement,
        schemaName: "ReviewResult",
        schemaDescription: "审核结果")
};
var chatResponse = await chatClient.GetResponseAsync(messages, options, ct);
```

或使用泛型：

```csharp
AgentRunResponse<ReviewResult> typed = await agent.RunAsync<ReviewResult>(userMessage, thread);
```

### 4. 多模态（图片/文件）

```csharp
var message = new ChatMessage(ChatRole.User, [
    new TextContent("请描述这张图"),
    new UriContent(new Uri("https://example.com/a.png"), mediaType: "image/png"),
    // 或字节流：new DataContent(imageBytes, mediaType: "image/png")
]);
```

### 5. Workflows — Sequential / Concurrent / Handoff / HITL

```csharp
using Microsoft.Agents.AI.Workflows;

var workflow = new WorkflowBuilder(translatorAgent)
    .AddEdge(translatorAgent, rewriterAgent)
    .AddEdge(rewriterAgent, reviewerAgent)
    .Build();

await foreach (var evt in workflow.RunStreamAsync(input, ct))
{
    // evt 可以是 ExecutorInvokedEvent / ExecutorCompletedEvent /
    // RequestInfoEvent（HITL）/ OutputStreamingEvent
}
```

**Handoff**：使用 `WorkflowBuilder.AddHandoff(sourceAgent, targetAgent, condition)` 让一个 agent
根据工具调用决定交由目标 agent 处理。

**Human-in-the-Loop**：executor 里调 `ctx.RequestInfo<T>()`，
外层在 `RequestInfoEvent` 到来时收集输入，再通过 `workflow.ResumeAsync(requestId, payload)` 续跑。

**Checkpointing**：`WorkflowBuilder.WithCheckpointProvider(...)` 或 `InProcessExecution.RunWithCheckpointsAsync`。

### 6. Memory / AgentThread

- **Window**：只把最近 N 条 ChatMessage 传给 LLM（在发起 `RunAsync` 前自行裁剪）。
- **Summary**：将旧消息摘要成一条 system 消息，常见做法是每 K 条或 T tokens 触发一次
  `summarize(messages)` 并替换旧消息。
- `AgentThread.DeserializeAsync(...)` / `SerializeAsync(...)` 可持久化。

### 7. MCP 工具集成

```csharp
using ModelContextProtocol.Client;

var transport = new HttpClientTransport(new HttpClientTransportOptions
{
    Endpoint = new Uri(server.Endpoint),
    TransportMode = HttpTransportMode.AutoDetect,
});
await using var client = await McpClient.CreateAsync(transport, cancellationToken: ct);
var result = await client.CallToolAsync(toolName, arguments, cancellationToken: ct);
```

### 8. Observability

自定义 ActivitySource：

```csharp
internal static class AgentTelemetry
{
    public const string SourceName = "AIAgent";
    public static readonly ActivitySource Source = new(SourceName);
}

using var activity = AgentTelemetry.Source.StartActivity("Agent.Run");
activity?.SetTag("agent.id", agent.Id);
activity?.SetTag("agent.model", agent.ModelId);
```

开启 GenAI 内置遥测：
```
OTEL_INSTRUMENTATION_GENAI_CAPTURE_MESSAGE_CONTENT=true
```

## 本仓库设计约定

- `CoreMod.Services.ExtensionsAIModelClient` 仍作为**低层 provider 路由+基础调用**实现，不直接被业务层新代码使用。
- 新业务层首选 `AIAgentMod.Services.MafAgentRuntime` → `ChatClientAgent`。
- 工具注册集中在 `AIAgentMod.Services.AgentToolFactory`（RAG、MCP、内置）。
- 所有新代码经 `ActivitySource("AIAgent")` 打点，便于 Aspire dashboard 追踪。
- Foundry Local 通过 `AIModelProvider.ProviderType == FoundryLocal` 走本地 endpoint 解析（`FoundryLocalEndpointResolver`），API Key 留空或填 `local`。

## 本地文档镜像

本地化的关键 MAF 文档放在 `docs/maf/`：
- `docs/maf/01-overview.md`
- `docs/maf/02-agents.md`
- `docs/maf/03-tools-mcp.md`
- `docs/maf/04-workflows-handoff-hitl.md`
- `docs/maf/05-structured-output-multimodal.md`
- `docs/maf/06-memory-observability.md`

## 重要规则

- **新能力一定要用 MAF 1.1 官方 API**，不要再自己实现多轮 tool-calling 循环；`ChatClientAgent` 已内置。
- **函数参数/返回值用强类型**，让 `AIFunctionFactory.Create` 自动推断 JSON schema。
- **避免混用"历史消息 List"与 `AgentThread`**；一次会话用一个 Thread。
- **所有 agent 调用前**都应包 `ActivitySource` span。
- **provider 接入优先用 OpenAI 兼容协议**（DeepSeek / Qwen / Foundry Local 都兼容），只有在必要时才走 Azure/Anthropic/Google 原生 SDK。
