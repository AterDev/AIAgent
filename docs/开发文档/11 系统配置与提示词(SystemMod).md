# 11 系统配置与提示词（SystemMod）

## 目标

复用 `SystemConfig` 进行提示词管理，为文件解析训练、Agent、工作流等提供统一可配置的提示词与模板。

## 步骤

1. **SystemConfig 扩展约定**
   - 配置分组：Prompt/Rag/Agent/Workflow 等。
   - 配置项：Name、Value、Description、IsSystem。

2. **提示词管理接口**
   - SystemConfigManager 提供按分组查询与更新。
   - 支持模板占位符（如 {{input}}、{{context}}）。

3. **调用场景集成**
   - KnowledgeBaseMod 文件解析/训练时引用提示词。
   - AIAgentMod 执行时可选择提示词模板。
   - WorkflowMod 步骤中引用提示词模板。

4. **审计与安全**
   - 配置变更记录与审计。
   - 系统级配置只允许管理员修改。

## 验收要点

- 提示词可配置、可分组查询。
- 业务模块可统一复用提示词配置。
