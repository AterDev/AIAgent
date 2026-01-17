# 06 MCP 工具框架与内置工具（McpMod）

## 目标

构建 MCP 工具注册、权限校验与执行管道，提供基础内置工具。

## 步骤

1. **工具实体与 DTO**
   - McpTool、ToolCallRecord 实体落位。
   - ToolDefinitionDto 用于对外暴露工具定义。

2. **工具注册与管理**
   - ToolManager：CRUD、启用/禁用、版本管理。
   - 支持 builtin/external/custom 类型。

3. **执行管道**
   - 参数校验（schema 校验）。
   - 权限检查（应用维度控制）。
   - 超时控制与取消。
   - 审计记录写入。

4. **内置工具实现**
   - query_knowledge_base：调用 RAG 检索。
   - execute_sql_query：只读、安全 SQL 执行。
   - http_request：允许列表域名的安全请求。

5. **扩展机制**
   - MCP Server 连接（stdio/HTTP/WebSocket）。
   - 通过配置注册 MCP Server，包含访问密钥等信息。
   - 添加 MCP Server 时校验访问密钥。
   - 工具能力缓存与健康检查。

## 验收要点

- MCP 工具可注册、查询、执行。
- 每次调用均有审计记录。
- 内置工具可被 Agent 调用并返回结果。
