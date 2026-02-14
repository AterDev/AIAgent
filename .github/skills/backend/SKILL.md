---
name: backend
description: ASP.NET Core / EF Core / Aspire 后端开发规范与最佳实践。
---

## 何时使用

任何涉及到后端逻辑和项目架构的内容。

## 项目结构层级

```sh
src/
├── Definition/
│   ├── Entity/              # 实体定义（按模块分文件夹）
│   ├── EntityFramework/     # EF DbContext 和迁移
│   ├── Share/               # 共享常量、扩展、服务
│   └── ServiceDefaults/     # 服务注册和中间件
├── Modules/
│   └── {ModuleName}/
│       ├── Managers/        # 业务逻辑层
│       └── Models/          # DTO 定义（按实体分文件夹）
│       └── Services/        # 模块内服务（可选）
└── Services/
    ├── ApiService/          # 公共 API
    ├── AdminService/        # 管理后台 API
    └── MigrationService/    # 数据库迁移服务
```

### Share共享项目

与业务无关的通用工具、常量、服务等都应该放在Share项目中，以供所有模块和服务复用。

- 只包含通用的逻辑算法等，不涉及任何业务数据的操作
- `Share/Services`封装通用的第三方库的调用，如缓存/邮件/消息队列等.
- 包含多语言文件，源代码生成器会自动生成常量到`Localizer.cs`，在多语言实现时，一定要使用Localizer的常量，避免硬编码字符串。


### Modules 

包含根据业务拆分的模块项目，每个模块包含 Manager（业务逻辑）和 Models（DTO 定义）。模块内可以有自己的 Services（可选），但要避免跨模块直接调用 Manager，应该通过共享服务或事件解耦。

- CoreMod，包含核心的基础设施和通用服务，可以被所有模块依赖。比如多个模块都要调用的逻辑，由于模式之间不可互相依赖，通过CoreMod提供服务来解耦。
- 模块主要是通过继承 ManagerBase<T> 来实现业务逻辑，所有Manager都要继承 ManagerBase<T> 或 ManagerBase
- 在模块中业务验证错误，抛出 `BusinessException`

模块之间不能相互依赖，除了`CoreMod`块可以被所有模块依赖外，其他模块只能依赖`Share`和`ServiceDefaults`。

### Services

包含具体的 API 服务项目，通过调用 Manager 来执行业务逻辑。

如果Controller没有绑定特定的Manager，则继承`RestControllerBase`的其他基类。

在Controller层，做用户输入验证和权限验证，不做业务验证，返回 `Problem()` / `NotFound()` 等 HTTP 错误响应。

### 返回值
- **成功**：`ActionResult<T>` 或直接返回类型
- **错误**：使用 `Problem()` 或 `NotFound()`，直接使用多语言常量作为错误消息，而不要new一个对象或使用字符串硬编码。
- **参数绑定**：有歧义时使用显式特性 `[FromBody]` / `[FromQuery]` / `[FromRoute]`


**后台任务服务**

- 不直接使用`DbContext`，也不要直接调用Manager，而是通过`DbContextFactory`创建DbContext实例，来执行业务逻辑。
- 优先复用`IEntityTaskQueue`或`IBackgroundTask`来实现队列

### 项目依赖层次（从下到上）

1. **Entity** → 定义数据模型
2. **EntityFramework** → 配置 DbContext，依赖 Entity
3. **Share + ServiceDefaults** → 共享工具和服务注册，依赖 EntityFramework
4. **Modules** → 模块的业务逻辑实现和DTO，依赖 Entity 和 Share
5. **Services** → API 控制器，依赖 Modules

<rules>

- 使用文件作用域命名空间
- 使用主构造函数，并将依赖注入到构造函数参数中，减少属性注入和字段注入
- 使用集合表达式
- 优先使用异步编程，并传递 `CancellationToken`
- 使用全局命名空间和中央包管理
- CoreMod块可以被所有模块依赖，其他模块之间不能相互依赖。
- Manager内，不要直接调用其他模块的Manager，而是通过共享服务或事件解耦。
- Controller 不能直接访问 DbContext
- Entity 不包含业务逻辑（仅数据模型和验证注解）
- 非必要切勿定义接口，除非明确有多个实现。接口必须放到单独的目录`Abstraction`中
- 优先使用`class`而不是`record`，除非确实需要不可变对象和结构化的值对象语义
- 优先使用`Perigon.AspNetCore.Utils.Extensions` 中的扩展方法`Merge/MapTo`进行映射。
</rules>

## 开发流程

1. 定义层，即实体的定义，DbContext的处理，以及共享服务的编写(封装以便简化和复用)
2. 模块层，即Manager和DTO的生成和编写; 并检查是否添加新的服务注入等。
3. 服务层，即Controller的生成和编写
4. **执行构建验证**（必须步骤）：验证编译无错误
5. 判断是否要在执行迁移，如果添加或修改了实体，则执行`scripts/EFMigrations.ps1`脚本，永远不要手动修改迁移文件。

**新加实体模型业务时，优先使用MCP工具`Perigon`，生成或创建模块/Entity/DTO/Manager/Controller等内容。**

### 多租户

架构同时支持多租户和单租户，从AppHost的`appsettings.Development.json`配置中，可以知道当前是单租户还是多租户模式。如果是单租户模式，tenantId默认为Guid.Empty。

### Aspire集成 

`AppHost`程序集为Aspire启动项目。

优先使用Aspire生态提供的功能和中间件，可通过MCP工具搜索微软官方文档了解如何使用。