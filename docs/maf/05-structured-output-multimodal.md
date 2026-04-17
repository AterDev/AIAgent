# Structured Output / Multimodal

## Structured Output

配置 `AIAgent.ResponseSchemaJson`：

```json
{
  "type": "object",
  "properties": {
    "score":       { "type": "integer", "minimum": 1, "maximum": 5 },
    "issues":      { "type": "array", "items": { "type": "string" } },
    "finalText":   { "type": "string" }
  },
  "required": ["score", "issues", "finalText"]
}
```

`MafAgentRuntime` 自动把它包装成 `ChatResponseFormat.ForJsonSchema(...)`。

调用方可用 `JsonSerializer.Deserialize<ReviewResult>(agentResponse.Text!)` 得到强类型对象。

## Multimodal 消息

`ChatMessage` 新增三字段：
- `ContentType` = `Text` | `Image` | `File`
- `AttachmentUrl` — 仅持久化 `https://...` / 对象存储 URL（走 `UriContent`）
- `AttachmentMime` — 如 `image/png`、`application/pdf`

调试/即时调用链路里的 `data:image/...;base64,...` 只在内存请求对象中传递，不进入 `ChatMessage` 实体持久化。

`MafAgentRuntime.PrepareHistory` 会把非 Text 消息转换为多段 `AIContent`：

```
[new TextContent("请分析这张图"), new UriContent(uri, "image/png")]
```

Agent/模型侧要求：`AIModelInfo.SupportsVision == true`（或对应 provider 支持多模态）。
