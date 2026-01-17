# 07 Agent 管理与执行（AIAgentMod）

## 目标

实现 Agent 的配置、执行与运行监控，支持工具调用与流式输出。

## 步骤

1. **实体与 DTO**
   - Agent、AgentExecution 实体。
   - DTO：AgentDetail/AgentAdd/AgentUpdate/AgentItem/AgentFilter。

2. **Agent 管理**
   - AgentManager：CRUD、模板、标签、启用状态。
   - 版本号管理与发布策略。

3. **执行引擎**
   - 构建消息上下文与系统提示词。
   - 调用模型（走统一模型调用层）。
   - 解析工具调用并调用 MCP 工具。
   - 保存执行记录与上下文。
   - 通过应用配置限定可用模型与工具。
   - 系统提示词可选用 SystemConfig 中的模板配置。

4. **流式响应**
   - WebSocket 推送执行过程。
   - 支持工具调用节点状态提示。

5. **监控与审计**
   - 记录执行耗时、token、工具调用次数。
   - 失败/超时错误归档。

## 验收要点

- Agent 可创建与执行。
- 工具调用可被正确解析与执行。
- 运行记录可查询与追踪。
