# GitHub Copilot Instructions

本仓库是.NET解决方案。是基于`Perigon.templates`模板的WebApi项目。在使用GitHub Copilot时，请遵循以下指导原则和偏好设置。

## 总体指导原则

- 准确和确定性为第一原则。
- 没有明确要求下，不要对项目进行build操作。
- 生成代码后，要进行检查，在错误列表/输出日志/编辑器报错中检查本次功能相关的内容，并进行修复。
- 没有要求的情况下，不要生成任何总结/更新/测试相关的文档。
- 后台多语言支持，会有源代码生成器自动生成常量内容，直接使用Localizer调用。


## 关键技术栈
1. 主要语言是:C# 14，前端是TypeScript，在代码提示时使用最新语法
2. 后端基于ASP.NET Core 10 和EF Core 10构建
3. 前端使用Angular 21+


## AI skills 规范入口
- 后端规范: .github/skills/backend/SKILL.md
- 前端规范: .github/skills/angular/SKILL.md

## Agent角色定义
- 后端开发: .github/agent/backend.md
- 前端开发: .github/agent/frontend.md
- 文档撰写: .github/agent/docs.md
- 架构/性能/代码审查: .github/agent/architect.md
- 通用全栈: .github/agent/agent.md

更多细节规范请直接查阅对应的Agent或skill文件；本文件仅提供总则和入口链接。
