# AIAgent Codex Instructions

本仓库是基于 `Perigon.templates` 的 .NET Web API 解决方案，使用 `Perigon.CLI` 做项目脚手架、代码生成和 OpenAPI 客户端生成。

## 总体原则

- 准确性和有效性优先，保持高效与严谨。
- 修改代码前先理解现有实现和项目模式，避免猜测。
- 命名要清晰简洁，并保持与项目风格一致。
- 对生成或修改的代码做自查，重点检查符号、语法、命名空间、依赖、类型和运行路径。
- 没有明确要求时，不新增总结、更新说明、测试报告等文档。
- 优先使用 `pwsh`、`dotnet`、`pnpm` 和项目已有工具链；少用临时脚本，确需使用时任务结束后清理。

## 技术栈

- C# 14
- Aspire 13+
- ASP.NET Core 10
- EF Core 10
- Angular 21+
- Windows 11 开发环境，优先使用 PowerShell 和 .NET 工具链。

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

- **Perigon** 和 **Aspire** 是本仓库最重要的工具。
- 涉及脚手架、模块/服务添加、代码生成、OpenAPI 客户端生成、MCP 配置时，优先使用 Perigon 相关能力或项目已有模式。
- 涉及分布式应用启动、资源状态、日志、链路、集成配置、运行态验证时，优先使用 Aspire CLI 和 `.agents/skills/aspire*`。
- 后端相关任务优先使用 `.agents/skills/backend/SKILL.md`。
- 前端相关任务优先使用 `.agents/skills/angular/SKILL.md`。
- 完整工程实现闭环任务可使用 `.agents/skills/engineer/SKILL.md`。
- 需要官方事实或新 API 时，优先查官方文档或一手来源；不要凭记忆猜测。

## 验证

- 验证方式要匹配任务风险和项目运行方式。
- Aspire 管理的服务优先通过 `aspire resource <name> rebuild/restart`、`aspire logs`、`aspire otel logs` 等运行态方式验证。
- 避免在 Aspire 正持有输出文件锁时反复直接 `dotnet build`；如果需要普通构建，先确认运行态和文件锁情况。
- 前端运行在 Aspire/开发服务器中时，优先查看 Aspire 的 frontend 日志和热更新结果。

## 清理

- 任务产生的临时脚本、日志、诊断文件、一次性文档等要在完成后清理。
- 不要修改或回滚与当前任务无关的用户改动。

## 问题解决

像工程师一样解决问题：

1. 先从代码和项目约定中定位事实。
2. 必要时查官方文档、GitHub 或项目 MCP/技能参考。
3. 每一行代码都要有明确目的，避免为了“可能有用”而扩大改动。
