# 09 开放平台 API 与权限

## 目标

提供统一的对外 API，满足模型管理、RAG、Agent、MCP、工作流的调用需求，并具备安全与审计能力。

## 步骤

1. **API 设计**
   - 按资源划分端点，遵循 RESTful。
   - 版本化路径：/api/v1/。

2. **主要端点**
   - 应用管理：/api/v1/apps
   - 模型管理：/api/v1/models
   - 知识库/RAG：/api/v1/rag/documents、/api/v1/rag/search
   - Agent：/api/v1/agents、/api/v1/agents/{id}/execute
   - MCP 工具：/api/v1/tools
   - 工作流：/api/v1/workflows、/api/v1/workflows/{id}/execute
   - 系统配置（提示词）：/api/v1/system-configs

3. **认证与鉴权**
   - OAuth2/JWT/API Key 统一支持。
   - 应用级权限控制（应用可配置可用模型与工具）。

4. **速率限制与审计**
   - 基于应用的限流。
   - 所有调用写入审计日志。

5. **OpenAPI 文档**
   - 自动生成 OpenAPI 3.1。
   - 完善请求/响应与错误说明。

## 验收要点

- API 端点可按模块独立调用。
- 权限与限流生效。
- OpenAPI 文档可用。
