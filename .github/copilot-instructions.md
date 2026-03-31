# GitHub Copilot Instructions

本仓库是.NET解决方案。是基于`Perigon.templates`模板的WebApi项目。并使用`Perigon.CLI`工具进行项目脚手架搭建和代码生成。

## 总体指导原则

- 准确和有效性为第一原则，保持高效与严谨。
- 在完全工作后，要通过实际构建或测试来进行验证。
- 类/方法/变量命名要清晰简洁，且保持与项目风格一致。
- 对生成的代码进行自我检查，避免符号、语法、命名空间、依赖等错误。
- 没有要求的情况下，不要生成任何总结/更新/测试相关的文档。
- 擅用`pwsh`和`.cs`脚本提高效率，以及IDE自身功能进行代码重构，终端要复用或用完关闭，减少相互影响。

## 关键技术栈
1. 基于最新的C# 14语言特性
2. 后端强依赖于：Aspire 13+,ASP.NET Core 10,EF Core 10
3. 开发环境：Win 11，VSCode或VS，可充分利用`pwsh`以及`dotnet`工具链.

## 项目结构

- 前端在 `src/ClientApp/WebApp` 
- 后端接口服务在 `src/Services` 
- 实体定义在 `src/Definition/Entity`
- 业务逻辑在 `src/Modules`，按模块划分
- Share共享项目在 `src/Definition/Share`
- 服务扩展在 `src/Definition/ServiceDefaults`
- 文档在 `docs/`
- 脚本在 `scripts/`
- 测试在 `tests/`
- razor模板在 `templates/`

## CLI和MCP工具

- **Perigon** 和 **Aspire** 是本仓库最重要的工具，凡是任务相关时应优先考虑并主动使用。
- 涉及项目脚手架、模块/服务添加、代码生成、OpenAPI客户端生成、MCP配置时，优先使用 **Perigon** 相关能力。
- 涉及分布式应用启动、资源状态检查、日志/链路排查、集成配置时，优先使用 **Aspire** 相关能力。
- Perigon提供代码生成和示例代码，优先使用它提供的能力，以获取更加准确的参考。
- 充分利用`Microsoft Learn`和`Github`等MCP工具搜索和解决问题。以及`PlayWright`进行自动化测试等。

## 清理

对于任务中产生的临时脚本/log/文档等内容，请在任务完成后进行清理，确保仓库整洁。

## 问题解决

像工程师一样解决问题：

1. 从相关技术的官方文档中查询解决方案
2. 通过Web搜索，尤其是GitHub中寻找类似问题的解决方案
3. 从不猜测代码，确保每一行代码都有明确的目的