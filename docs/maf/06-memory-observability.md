# Memory / Observability

## Memory

通过 `AIAgent.MemoryMode` + `AIAgent.ContextWindow` 控制：

| Mode | 行为 |
|---|---|
| `None` | 每次请求无历史（纯函数式） |
| `Window` | 末 N 条消息（N = ContextWindow，默认 20） |
| `Summary` | 末 N 条 + 更旧消息的简要占位摘要（未来用 LLM 摘要替换） |

`Conversation` 实体已有 `SystemPrompt / ModelName`，未来可扩展 `MemorySummaryJson + LastSummarizedAt`
做增量摘要持久化。

## Observability

- `ActivitySource` 统一命名 `AIAgent`（`AgentTelemetry.Source`）
- `ServiceDefaults.Extensions.cs` 已加入 `.AddSource("AIAgent")` / `.AddSource("Microsoft.Agents.AI")`
- 环境变量 `OTEL_INSTRUMENTATION_GENAI_CAPTURE_MESSAGE_CONTENT=true`（AppHost 已默认开启）
  会让 MAF / Extensions.AI 记录完整消息内容，方便 Aspire Dashboard 复盘

典型 span：

```
Agent.Run             tags: agent.name, agent.model, gen_ai.system
  ├─ Tool.Invoke      tags: tool.name
  └─ (内置) Microsoft.Extensions.AI
Workflow.Step         tags: workflow.step_name, workflow.step_type
```
