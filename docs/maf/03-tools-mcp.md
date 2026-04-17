# Tools / MCP

## 内置业务 Tools

通过 `AgentToolFactory`：

```csharp
var ragTool = agentToolFactory.CreateRagQueryTool(defaultCollectionId: kb.Id);
var mcpRouter = agentToolFactory.CreateGenericMcpTool();  // 通用 MCP 路由
```

## MCP Tool 接入

本仓库已有：
- `McpClientProvider`（`McpMod.Services`）缓存每个 `MCPServerInfo` 的 `McpClient`（HTTP/SSE/stdio）
- `McpToolExecutor`（`McpMod.Services`）处理权限检查 + 30s 超时 + 内容解包
- `BuiltinToolExecutor` 实现 `query_knowledge_base` / `execute_sql_query` / `http_request`

`AgentToolFactory.CreateGenericMcpTool()` 将 `IMcpToolExecutor` 暴露为 `mcp_call(name, argumentsJson)`，
适合让模型在未知 MCP Tool 列表时走统一通道。

正式生产建议：为每个具体 MCP Tool 通过
`AIFunctionFactory.CreateDeclaration(name, description, jsonSchema)` 生成强类型
的 `AITool`，以便模型直接在参数层面正确调用。

## 自建业务函数

```csharp
AITool sendEmailTool = AIFunctionFactory.Create(
    async (string to, string subject, string body, CancellationToken ct) =>
    {
        await mailService.SendAsync(to, subject, body, ct);
        return new { success = true };
    },
    name: "send_email",
    description: "发送邮件");
```
