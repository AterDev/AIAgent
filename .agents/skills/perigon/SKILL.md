---
name: perigon
description: >-
  Perigon 主入口技能：用于基于 Perigon CLI/MCP 进行项目脚手架、模块与服务生成、DTO/Manager/Controller/Request 客户端生成、MCP 配置与 Studio 操作，并在后端与前端开发中按照模板约定完成代码生成与落地。Use when: any Perigon-based development task, scaffolding, code generation, module/service creation, API client generation, MCP setup, Studio usage, backend/frontend implementation that should follow Perigon conventions.
---

# Perigon Skill

This repository uses `Perigon.CLI` for scaffolding, code generation, and MCP integration.
This is the single top-level Perigon skill. Backend and Angular guidance is consolidated into project-local references so the main skill stays focused on the actual Perigon workflow.

## When to use this skill

Use this skill for nearly all Perigon-related development work, especially when the task involves:
- 创建新解决方案、模块、服务，或启动 Studio；
- 生成或更新 DTO、Manager、Controller、Entity、Request Client、前端请求模型；
- 按照 Perigon 约定完成后端/前端代码骨架与业务代码生成；
- 安装、列出、打包模块包，或初始化/启动 MCP 服务；
- 需要先查看 CLI 帮助再执行命令，避免猜测参数；
- 涉及后端架构与业务逻辑、或者 Angular 页面/组件/请求客户端的实现与 review。

## Project structure

```sh
src/
├── AppHost/                 # Aspire 启动入口与资源编排
├── Definition/
│   ├── Entity/              # 实体定义（按模块分文件夹）
│   ├── EntityFramework/     # EF Core DbContext 与迁移
│   ├── Share/               # 共享常量、扩展、服务
│   └── ServiceDefaults/     # 服务注册与中间件
├── Perigon/                 # 基础类库、工具扩展与源生成项目
├── Services/
│   ├── ApiService/          # 公共 API
│   ├── AdminService/        # 管理后台 API
│   └── MigrationService/    # 数据库迁移服务
└── ClientApp/WebApp/        # 前端应用
```

## Reference routing

| Task area | Reference |
|---|---|
| Perigon CLI / MCP / Studio commands | [references/perigon-cli.md](references/perigon-cli.md) |
| Backend architecture and service patterns | [references/backend.md](references/backend.md) |
| Angular frontend development | [references/angular.md](references/angular.md) |

## Working principles

- **Use Perigon for scaffolding and code generation**, not for build/test/run.
- **Prefer generated code over hand-written boilerplate** for modules, services, DTOs, managers, controllers, and request clients.
- **For backend work**, keep business logic in managers, keep controllers focused on validation/authorization/HTTP responses, and reuse shared logic from `src/Perigon` before adding local duplicates.
- **For frontend work**, prefer Angular standalone components, Material, signals, typed forms, and i18n keys; keep generated request contracts aligned with backend OpenAPI definitions.
- **Check help before guessing options** with `perigon -h` or `perigon <command> -h`.
- **Prefer the current subcommand form** (`perigon module ...`) over older shorthand aliases when the help output is ambiguous.

## Verification guidance

- Backend changes: run `dotnet build` for the affected project or solution.
- Frontend changes: run `pnpm build` and use `pnpm start` for live validation when the page is non-trivial.
