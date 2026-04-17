# Microsoft Agent Framework 1.1 补强说明

本文档记录本次基于 Microsoft Agent Framework (MAF) 1.1 的功能补强范围、验证步骤与待办事项。

## 一、目标与范围

为现有 AIAgent/Workflow 模块补齐以下能力（全部面向兼容 OpenAI 的 Provider，包括 DeepSeek、Foundry Local、OpenAI-compatible 自定义）：

- Skill 定义 / Handoff 路由 / Human-in-the-Loop 钩子
- Multimodal 输入（Image / File）
- RAG Tool
- Structured Output（JSON Schema ResponseFormat）
- MCP Tool 工厂
- Memory 策略（Window / Summary）
- Observability（ActivitySource = `AIAgent` / `Microsoft.Agents.AI`）
- Background Responses（通过 Workflow `agent_call` 步骤 + executionId 轮询）

## 二、本次交付

### 1. 文档

- [.github/skills/maf/SKILL.md](.github/skills/maf/SKILL.md) — MAF 1.1 技能速查
- [docs/maf/README.md](docs/maf/README.md) 及 01–06 专题：架构映射 / Agent / Tool+MCP / Workflow-Handoff-HITL / Structured+Multimodal / Memory+Observability

### 2. 实体扩展

- `AIAgent`: 新增 `ProviderId`/`Temperature`/`TopP`/`FrequencyPenalty`/`PresencePenalty`/`MaxOutputTokens`/`Capabilities`(AgentCapabilities Flags)/`MemoryMode`/`ContextWindow`/`ResponseSchemaJson`/`IconUrl`/`OutputLanguage`/`Skills`/`HandoffTargets`/`Tags`
- `ApplicationAgent`: 同步新增上述可覆盖字段
- `ChatMessage`: 新增 `AttachmentUrl`/`AttachmentMime`/`AttachmentName`（多模态；仅持久化远程 URL，不存 data URI）
- `AIModelProvider`: 新增 `ProviderType` (OpenAiCompatible/FoundryLocal/AzureOpenAI/Anthropic/Google/Custom)、`IsEnabled`
- `AIAgentEnums.cs`: `[Flags] AgentCapabilities`、`AgentMemoryMode`

### 3. Core 基础设施

- `ExtensionsAIModelClient.GetChatClientAsync(model, provider?, ct)` — 对外暴露 `IChatClient`（MAF 直接消费）
- `ExtensionsAIModelClient.IsProviderConfigured` — `FoundryLocal` 允许空 ApiKey，回退 `"not-required"`
- `QdrantOptions.EmbeddingModel` — 可通过配置切换 embedding 模型
- `CoreModelEmbeddingGenerator.GenerateAsync(text, modelName?, size, ct)` — 按模型动态路由
- `ModelRoute.ProviderType` + `DbModelRouter` 填充

### 4. MAF 运行时

- `src/Modules/AIAgentMod/Services/Maf/AgentTelemetry.cs` — ActivitySource (`AIAgent` 1.0.0) + helper span（Agent.Run / Tool.Invoke / Workflow.Step）
- `src/Modules/AIAgentMod/Services/Maf/AgentToolFactory.cs` — RAG Tool + MCP Tool 工厂
- `src/Modules/AIAgentMod/Services/Maf/MafAgentRuntime.cs`
  - `BuildAgentAsync(agent, tools?)` → `MafAgentBundle(ChatClientAgent, ChatOptions)`
  - `PrepareHistory(agent, history)` → 按 MemoryMode 裁剪，产出 `IReadOnlyList<Microsoft.Extensions.AI.ChatMessage>`（含多模态 Data/Uri 内容）
  - `ChatOptions` 自动装配 Temperature/TopP/MaxTokens/FrequencyPenalty/PresencePenalty/Tools/ResponseFormat(JSON Schema)
- DI 注册已加入 `AIAgentMod.ModuleExtensions`

### 5. Observability

- `ServiceDefaults.Extensions.ConfigureOpenTelemetry` 已订阅 `AIAgent` + `Microsoft.Agents.AI`

### 6. Provider / Seed

- `MigrationService.Worker`:
  - DeepSeek ApiKey 改为从环境变量 `AIAgent__Seed__DeepSeekApiKey` 读取（未提供则留空）
  - 新增 **FoundryLocal** provider（`http://localhost:5273/v1`）含 `qwen3-0.6b` 对话模型 + `qwen3-embedding-0.6b` embedding（1024 dim）
  - 新增 `SeedTranslationWorkflowAsync`：`TranslationPipelineDemo` Workflow，链式 3 步 `agent_call`：`DemoTranslatorAgent` → `DemoRewriterAgent` → `DemoReviewerAgent`（`Reviewer` 开启 StructuredOutput）

### 7. 脚本

- [scripts/SetupFoundryLocal.ps1](../../scripts/SetupFoundryLocal.ps1) — `winget` 安装 Foundry Local、启动服务、拉取 qwen3-0.6b + qwen3-embedding-0.6b

### 8. 迁移

- 已删除历史 EF Migrations，重新生成 `InitialCreate`。

## 三、验证步骤

### 准备（首次）

```powershell
# 1. 安装 Foundry Local + 下载模型
pwsh ./scripts/SetupFoundryLocal.ps1

# 2. 确认 qwen3-embedding-0.6b 的维度是 1024（SetupFoundryLocal 结束会打印）
#    如为 1024，需在 appsettings 覆盖 Qdrant:VectorSize = 1024

# 3. 启动 Aspire（首次会应用新的 InitialCreate Migration）
dotnet run --project src/AppHost
```

### 验证 A：DeepSeek 对话（已 seeded apiKey）

- 打开 AdminService → 管理 Agent → 新建/使用已有 Agent 指定 Model = `deepseek-chat`
- 或直接调用 `/api/aiagentconversation` 走 DeepSeek 路径
- 成功标志：能正常返回对话，Trace 中能看到 `AIAgent` span + `Microsoft.Agents.AI` span（若已接入 MafAgentRuntime）

### 验证 B：Foundry Local RAG

- 上传文档 → KnowledgeBase 走 embedding 路径（CoreModelEmbeddingGenerator 会用 `QdrantOptions.EmbeddingModel`，需将其覆盖为 `qwen3-embedding-0.6b`）
- 创建 Agent 绑定 KnowledgeBase，通过 `AgentToolFactory.CreateRagQueryTool(collectionId)` 装配 RAG 工具

### 验证 C：Translator→Rewriter→Reviewer Handoff Workflow

- Workflow 名称 `TranslationPipelineDemo`（已 seeded）
- POST `/api/workflows/{id}/executions` 触发，input 里给 `text` 字段
- 现有 `WorkflowExecutor` 已支持 `agent_call` 类型 step 串联

## 四、已知 TODO（未在本次完成）

| 项 | 说明 |
| --- | --- |
| 前端 DTO/API 暴露 | 后端 DTO 已扩展；前端 Angular client 需 `perigon generate request` 刷新 |
| MafAgentRuntime 接入 | 现有 `EnhancedAgentExecutionService` 未替换为 MAF，保留旧路径以防回归；需后续切换 |
| HITL 端点 | 审批/回答的 REST 接口未新增；workflow step 可通过 `type: "approval"` 在 WorkflowExecutor 补齐 |
| Background Responses | 当前 Workflow 执行已是后台模式（executionId 轮询），但未暴露专用 REST |
| Structured Output 端点 | `ResponseSchemaJson` 字段已存在 DTO + Entity；Controller 透传已就绪，由 MafAgentRuntime 消费 |
| Foundry Local embedding / RAG 端到端测试 | Foundry Local catalog 当前仅提供 CPU 聊天/工具模型，**无 embedding 模型**；RAG 端到端仍需依赖 OpenAI/DeepSeek 兼容的 embedding provider。已就绪：聊天推理 `qwen3-0.6b-generic-cpu:4` 通过 `FoundryLocalChatTests.FoundryLocal_ShouldServeChat_ViaApplicationApiKey` 验证 ✅ |
| Qdrant VectorSize=1024 | 默认仍为 256；从 Foundry Local qwen 切换时需用户或种子覆盖配置 |
| 旧数据清理 | Postgres 容器开启了 `WithDataVolume()`；首次新迁移应用到已有库会冲突，需手工：`docker volume rm <name>` 或 `DROP DATABASE` 后 `aspire start` 重建 |

## 五、已验证

- `dotnet build AIAgent.slnx`: 0 error / 1 cref warning
- `ApiTest.AIAgentMod.MafAgentExtensionsTests`:
  - `AIAgent_WithMafExtensionFields_ShouldRoundTrip` ✅
  - `SeededTranslationAgents_ShouldFormHandoffChain` ✅
- `ApiTest.AIAgentMod.FoundryLocalChatTests`:
  - `FoundryLocal_ShouldServeChat_ViaApplicationApiKey` ✅（依赖本地 Foundry Local 服务 http://127.0.0.1:55655，未运行时自动跳过）

## 五、NuGet 依赖（Directory.Packages.props 已存在）

- `Microsoft.Agents.AI` 1.1.0
- `Microsoft.Agents.AI.OpenAI` 1.1.0
- `Microsoft.Agents.AI.Workflows` 1.1.0
- `Microsoft.Extensions.AI` 10.5.0
- `ModelContextProtocol` 1.2.0
- `OpenAI` 2.10.0
