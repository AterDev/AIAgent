# GitHub Copilot Instructions

本仓库是.NET解决方案。是基于`Perigon.templates`模板的WebApi项目。并使用`Perigon.CLI`工具进行项目脚手架搭建和代码生成。

## 总体指导原则

- 准确和确定性为第一原则。
- 没有明确要求下，不要对项目进行build操作。
- 对生成的代码进行自我检查，避免符号，语法，命名空间，依赖等错误。
- 没有要求的情况下，不要生成任何总结/更新/测试相关的文档。

## 关键技术栈
1. 基于最新的C# 14语言特性
2. 后端强依赖于：Aspire 13+,ASP.NET Core 10,EF Core 10
3. 开发环境：Windows11，可充分利用pwsh以及dotnet工具链

## Agent说明

本项目代码生成请使用以下Agent：

`.github/agents/engineer.md` - 资深全栈工程师，统一处理所有开发任务

## MCP工具

了解Perigon MCP工具，充分利用它提供的方法生成代码。如生成控制器、服务、DTO、实体等，添加模块等。
