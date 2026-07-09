# Backend Reference

This reference belongs to the Perigon skill and covers the backend architecture used by this template.

## When to use

Use this reference when the task involves:

- designing entities, DbContext, and shared services;
- implementing managers, DTOs, controllers, and service defaults;
- wiring business logic, migrations, and service-layer structure;
- reviewing backend implementation in a Perigon-based solution.

## Project structure

```sh
src/
├── Perigon/                 # 基础类库、工具扩展和源生成项目
├── Definition/
│   ├── Entity/              # 实体定义（按模块分文件夹）
│   ├── EntityFramework/     # EF Core DbContext 和迁移
│   ├── Share/               # 共享常量、扩展、服务
│   └── ServiceDefaults/     # 服务注册和中间件
├── Modules/
│   └── {ModuleName}/
│       ├── Managers/        # 业务逻辑层
│       ├── Models/          # DTO 定义
│       └── Services/        # 模块内服务（可选）
└── Services/
    ├── ApiService/          # 公共 API
    ├── AdminService/        # 管理后台 API
    └── MigrationService/    # 数据库迁移服务
```

## Core rules

- Keep business logic in managers instead of controllers.
- Prefer `ManagerBase<T>` / `ManagerBase` and `RestControllerBase` patterns.
- Keep controller layers focused on input validation, authorization, and HTTP response shaping.
- Use shared utilities from `src/Perigon` before adding local duplicates.
- Use Aspire and service defaults where relevant to the runtime model.
- Avoid direct `DbContext` access in controllers; prefer manager or factory-based flows.
- Use `BusinessException` for business validation errors and return `Problem()` / `NotFound()` for HTTP-level errors.
- Prefer `ActionResult<T>` or direct return types for success responses and use localized messages rather than hard-coded strings.

## Development flow

1. Define entities, DbContext, and shared services.
2. Implement modules and managers / DTOs.
3. Implement controllers and API endpoints.
4. Run build validation with `dotnet build` for the affected project or solution.
5. If entities are changed, review the migration workflow and scripts instead of manually altering generated migrations.

## Architecture guidance

- `src/Perigon/Perigon.AspNetCore` provides common web API base libraries and helpers, while `Perigon.AspNetCore.Toolkit` and `Perigon.AspNetCore.SourceGeneration` provide reusable tooling and source generation capabilities.
- Keep shared cross-cutting logic in `src/Definition/Share` or the shared service layer rather than duplicating it in modules.
- Put business logic in managers. Controllers should focus on validation, authorization, and shaping HTTP responses.
- Use `ManagerBase<T>` / `ManagerBase` and `RestControllerBase` patterns unless the project already has a more specific base class.
- For background work, prefer `DbContextFactory` plus `IEntityTaskQueue` or `IBackgroundTask` rather than directly using a `DbContext` in a worker path.
- Use the existing shared mapping helpers such as `Merge` / `MapTo` from the Perigon utilities instead of introducing ad-hoc mapping code.
- Respect the multi-tenant and single-tenant runtime model from the AppHost configuration; do not assume a single tenant mode when the app is configured otherwise.
- Keep module dependencies minimal. Avoid direct cross-module manager calls; prefer shared services or events.

## Code conventions

- Use file-scoped namespaces and primary constructors where appropriate.
- Prefer async programming and pass `CancellationToken` through the flow.
- Avoid interface-first design when there is only one implementation.
- Keep module-to-module dependencies minimal; prefer shared services or events for cross-module interaction.
- Follow the existing solution conventions such as global usings and central package management.

## Verification

- Run `dotnet build` after backend changes.
- If the change touches the domain model, review migration and startup behavior before concluding.
