# AGENTS

## 总体原则

- 从不猜测，确定性与准确是第一原则，保持高效与严谨。
- 代码和文档的可读性和可维护性优先，通过适当注释、清晰命名和局部变量降低复杂度。
- 没有明确要求时，不要在项目内生成内容总结、更新记录或测试报告类文档。
- 优先使用操作系统、IDE 和项目内已有工具与能力，例如 `pwsh`、`dotnet`、`pnpm`、代码搜索和重构工具。

## 技术栈

1. 基于最新的 C# 14 语言特性。
2. 后端强依赖于 Aspire 13+、ASP.NET Core 10、EF Core 10。
3. 前端默认使用 Angular，具体以仓库现有代码为准。

## 项目结构

- `src/ClientApp/WebApp`：前端 Angular 应用
- `src/Services`：后端接口服务
- `src/Definition/Entity`：实体定义
- `src/Definition/EntityFramework`：DbContext 和迁移
- `src/Definition/Share`：共享常量、扩展和基础能力
- `src/Definition/ServiceDefaults`：服务注册、中间件和默认服务能力
- `src/Modules`：业务模块，按模块划分 Manager、DTO、服务等
- `docs/`：文档
- `scripts/`：脚本
- `tests/`：测试
- `templates/`：Razor 模板

## 工具与技能

- 涉及项目脚手架、模块或服务添加、代码生成、OpenAPI 客户端生成、MCP 配置时，优先使用 `Perigon` 相关能力。
- 涉及分布式应用启动、资源状态检查、日志链路排查、集成配置时，优先使用 `Aspire` 相关能力；普通构建和测试优先使用 `dotnet build` 或 `dotnet test`。
- 需要前端功能验证时，优先结合 Playwright 或前端构建校验。

### 必读 skill

任何实现、规划、审查任务开始前，必须先读取以下两个skill：

- `.agents/skills/perigon/SKILL.md`
- `.agents/skills/aspire/SKILL.md`

### 主要技术 skill

- `perigon`：Perigon CLI、MCP、脚手架、代码生成与模板约定
- `aspire`：Aspire AppHost、资源编排、运行、观察与相关子工作流
- `dotnet-guidelines`：.NET 开发规范
- `angular-guidelines`：Angular 开发规范
- `commit-message`：提交信息生成规范
- `development-plan`：开发计划制定规范
- `playwright-cli`：前端自动化验证与页面交互
- `docs`：开发文档编写规范
- `code-review`：代码审查规范

## 验证

- 验证方式要匹配任务风险和项目运行方式。
- Aspire 管理的服务优先通过 `aspire resource <name> rebuild/restart`、`aspire logs`、`aspire otel logs` 等运行态方式验证。
- 避免在 Aspire 正持有输出文件锁时反复直接 `dotnet build`；如果需要普通构建，先确认运行态和文件锁情况。
- 前端运行在 Aspire/开发服务器中时，优先查看 Aspire 的 frontend 日志和热更新结果。

## 思维模型

作为以目标为导向的架构师编写代码：

1. 命名清晰、简洁、易理解，充分利用C#语言的表达能力和类型系统，写出结构清晰、可维护的代码。
1. 代码结构清晰，模块划分合理， 以面向对象思维定义和组织类和方法。
1. 方法和类符合单一职责原则，并补充必要说明。
