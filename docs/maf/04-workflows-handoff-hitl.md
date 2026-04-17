# Workflows / Handoff / HITL

## 本仓库现状

`WorkflowMod.Services.WorkflowExecutor` 已经使用 `InProcessExecution.RunAsync(...)`
运行本仓库自己的 JSON schema 驱动的 workflow（steps：agent_call / tool_call / rag / model / condition）。

目前 **Handoff** 通过顺序 `agent_call` 步骤 + `next` 指向目标 step 达成（参见
种子中的 `TranslationPipelineDemo`）。

## 未来扩展点

| 能力 | 位置 | 状态 |
|---|---|---|
| MAF 原生 `WorkflowBuilder.AddHandoff` | `WorkflowExecutor.BuildWorkflow` | 待接入 |
| Human-in-the-loop `ctx.RequestInfo<T>()` | 新增 step type `human_approval` | 待接入（现有 `WorkflowExecution.Status` 有 Paused 状态可承接） |
| Checkpointing `InProcessExecution.RunWithCheckpointsAsync` | 现有 `ContextJson + StepExecutionsJson` | 部分已实现 |

## 示例：Handoff 语义的顺序工作流（种子 `TranslationPipelineDemo`）

```jsonc
{
  "name": "TranslationPipelineDemo",
  "steps": [
    { "id": "translate", "type": "agent_call", "agentName": "DemoTranslatorAgent", "next": "rewrite" },
    { "id": "rewrite",   "type": "agent_call", "agentName": "DemoRewriterAgent",   "next": "review"  },
    { "id": "review",    "type": "agent_call", "agentName": "DemoReviewerAgent",   "next": null     }
  ]
}
```

## HITL 事件 schema（规划）

```
POST /api/workflows/{id}/executions            → 启动
GET  /api/workflows/executions/{id}/stream     → SSE，收到 request_info 事件时挂起
POST /api/workflows/executions/{id}/resume     → body: { requestId, payload }
```

对应实体字段：`WorkflowExecution.Status = Paused`，`ContextJson` 存储待审批数据。
