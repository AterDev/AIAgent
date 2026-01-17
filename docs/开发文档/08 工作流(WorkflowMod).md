# 08 工作流（WorkflowMod）

## 目标

实现工作流定义、执行与监控，支持多种步骤类型与容错策略，并优先复用 MAF 的工作流定义格式。

## 步骤

1. **实体与 DTO**
   - Workflow、WorkflowExecution 实体。
   - DTO：WorkflowDetail/WorkflowAdd/WorkflowUpdate/WorkflowItem/WorkflowFilter。

2. **定义校验**
   - 优先复用 MAF 的工作流定义格式。
   - 工作流 JSON schema 校验。
   - 步骤间引用与循环检测。

3. **执行引擎**
   - 顺序/并行/分支执行。
   - 变量上下文与数据映射。
   - 长流程异步执行与恢复。

4. **步骤实现**
   - agent_call：调用 Agent。
   - tool_call：调用 MCP 工具。
   - condition：条件分支。
   - loop：集合遍历/条件循环。
   - data_transform：模板与映射。
   - delay：延迟等待。

5. **监控与错误处理**
   - 步骤级执行记录。
   - 重试/降级/中止策略。

## 验收要点

- 工作流可发布并执行。
- 执行记录可追踪与回放。
- 异常策略生效。
