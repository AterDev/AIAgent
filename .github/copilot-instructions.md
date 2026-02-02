# GitHub Copilot Instructions

本仓库是.NET解决方案。是基于`Perigon.templates`模板的WebApi项目。并使用`Perigon.CLI`工具进行项目脚手架搭建和代码生成。

## 总体指导原则

- 准确和有效性为第一原则。
- **必须**通过实际构建验证代码：
  - 后端修改：执行 `dotnet build` 验证编译无错误
  - 前端修改：执行 `npm run build` 验证构建无错误
  - 本地开发时可辅助使用 Aspire MCP / get_errors 快速定位问题
- 构建-修复循环是代码编写的核心步骤，不可跳过。
- 对生成的代码进行自我检查，避免符号、语法、命名空间、依赖等错误。
- 没有要求的情况下，不要生成任何总结/更新/测试相关的文档。

## 关键技术栈
1. 基于最新的C# 14语言特性
2. 后端强依赖于：Aspire 13+,ASP.NET Core 10,EF Core 10
3. 开发环境：Windows11，可充分利用`pwsh`以及`dotnet`工具链.


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

## Agent说明

本项目代码生成请使用以下Agent：

`.github/agents/engineer.md` - 资深全栈工程师，统一处理所有开发相关的任务。

## MCP工具

项目高度依赖Perigon 工具来提供示例代码，请充分利用它提供的功能：

- 添加模块
- 生成实体
- 生成DTO
- 编写Manager业务逻辑代码
- 生成控制器Controller代码
- 创建Razor模板
- 执行生成任务
- 通过Razor模板生成代码(验证模板正确性)

## 清理

对于任务中产生的临时脚本/log/文档等内容，请在任务完成后进行清理，确保仓库整洁。

## 问题解决

像工程师一样解决问题：

1. 从相关技术的官方文档中查询解决方案
2. 通过Web搜索，尤其是GitHub中寻找类似问题的解决方案
3. 从不猜测代码，确保每一行代码都有明确的目的