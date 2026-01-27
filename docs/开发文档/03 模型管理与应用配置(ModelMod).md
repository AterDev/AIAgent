# 03 模型管理与应用配置（ModelMod）

## 目标

将模型管理与调用用量等业务逻辑统一放在 ModelMod，负责模型、提供商、应用、配额与调用记录。

## 步骤

1. **模型与提供商管理**
   - ModelProfile：模型名称、能力（embedding/chat/vision/tools）、最大上下文长度、是否支持 Responses API。
   - ModelProvider：渠道配置（域名 + API Key）、超时与重试策略。
   - 模型版本通过名称区分，无需额外版本实体。

2. **应用管理与权限**
   - Application：应用名称、密钥、启用状态、描述。
   - ApplicationQuota：应用级配额与限流规则。
   - ApplicationModelPermission：应用可用模型清单。
   - ApplicationToolPermission：应用可用 MCP 工具清单。

3. **调用用量记录与统计**
   - ModelInvocation：统一调用记录，包含 token 统计、耗时、状态、场景。
   - 字段：ApplicationId、ModelId、Scene、PromptTokens/CompletionTokens/TotalTokens、DurationMs、Status、ErrorMessage。
   - 按应用、模型、场景维度聚合统计。

4. **限流与配额**
   - 按应用维度限流与配额。
   - 支持 Redis 计数窗口。

5. **与 CoreMod 封装对接**
   - 统一调用入口由 CoreMod 提供。
   - ModelMod 负责写入调用记录与业务校验（配额/权限）。

6. **安全与审计**
   - 敏感配置加密存储。
   - 应用密钥生命周期管理与审计。

## 验收要点

- 模型列表可查询，支持启用/禁用与名称区分。
- 应用可配置配额与可用模型/工具。
- 调用记录完整可追踪。
