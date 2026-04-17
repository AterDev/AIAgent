# MAF 1.1 架构映射

## 本仓库如何映射到 MAF 1.1

| MAF 概念 | 本仓库实现 | 文件 |
|---|---|---|
| `IChatClient` | OpenAI 兼容 provider 路由 | `CoreMod.Services.ExtensionsAIModelClient.GetChatClientAsync` |
| `IEmbeddingGenerator<string, Embedding<float>>` | 同上 | `CoreMod.Services.ExtensionsAIModelClient.EmbeddingAsync` |
| `ChatClientAgent` | 业务层入口 | `AIAgentMod.Services.Maf.MafAgentRuntime.BuildAgentAsync` |
| `AITool`（业务 skill/RAG） | `AIFunctionFactory.Create(...)` 包装服务方法 | `AIAgentMod.Services.Maf.AgentToolFactory` |
| `AITool`（MCP 工具声明） | `AIFunctionFactory.CreateDeclaration(...)` | 同上 |
| `AgentThread` / 历史窗口 | 通过 `AIAgent.MemoryMode + ContextWindow` 裁剪 `ChatMessage` 列表 | `MafAgentRuntime.PrepareHistory` |
| `ChatResponseFormat.ForJsonSchema` | `AIAgent.ResponseSchemaJson` | `MafAgentRuntime.BuildChatOptions` |
| `DataContent` / `UriContent` | `ChatMessage.AttachmentUrl + AttachmentMime + ContentType` | `MafAgentRuntime.BuildContents` |
| `Workflow`（MAF Workflows）| `WorkflowMod.WorkflowExecutor` + JSON schema 驱动 | 参考 `04-workflows-handoff-hitl.md` |
| Observability `ActivitySource` | `AgentTelemetry.Source = new("AIAgent")` | `AIAgentMod.Services.Maf.AgentTelemetry` |

## Provider 分层

```
业务代码 (AIAgentMod) 
   │
   ▼ 使用 MAF
ChatClientAgent
   │
   ▼
IChatClient (Microsoft.Extensions.AI)
   │
   ▼ ExtensionsAIModelClient.GetChatClientAsync
OpenAIClient (OpenAI SDK)
   │
   ▼ Endpoint=<BaseUrl>  ApiKeyCredential
实际 provider: OpenAI / DeepSeek / Foundry Local / Qwen / Google (OpenAI 兼容模式)
```

## Foundry Local 特殊处理

1. `AIModelProvider.ProviderType == FoundryLocal` 时，`ExtensionsAIModelClient` 允许空 ApiKey，
   使用 `"not-required"` 占位符传给 OpenAI SDK。
2. 动态端口由 `scripts/SetupFoundryLocal.ps1` 启动后提示；种子值为 `http://localhost:5273/v1`。
3. 默认 embedding 模型 `qwen3-embedding-0.6b`（1024 维），需同步更新 `QdrantOptions.VectorSize`。
