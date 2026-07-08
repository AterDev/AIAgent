---
name: backend
description: ASP.NET Core / EF Core / Aspire 后端开发规范与最佳实践。
---

## 何时使用

任何涉及到后端逻辑和项目架构的内容。

## 项目结构层级

```sh
src/
├── Perigon/                 # 基础类库、工具扩展和源生成项目
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

### Perigon 目录

- `src/Perigon/Perigon.AspNetCore`：提供 Web API 基础类库、常用帮助类和通用扩展。
- `src/Perigon/Perigon.AspNetCore.Toolkit`：提供可复用的 Web API 工具和扩展能力。
- `src/Perigon/Perigon.AspNetCore.SourceGeneration`：提供源代码生成能力，用于模块注入、`Localizer` 常量等基础代码生成。

Perigon 包含基础的字符串/Queryable/对象映射/时间日期/加解密等扩展方法，要优先使用这些已提供的能力，避免重复实现。

### Share共享项目

- Shared constants live in src/Definition/Share/Constants; module-specific constants stay in their module/service.
- 只包含通用的逻辑算法等，不涉及任何业务数据的操作
- 封装通用的第三方库的调用，如缓存/邮件/消息队列等
- 包含多语言文件，源代码生成器会自动生成常量到`Localizer.cs`，在多语言实现时，一定要使用Localizer的常量，避免硬编码字符串。

### Modules 

包含根据业务拆分的模块项目，每个模块包含 Manager（业务逻辑）和 Models（DTO 定义）。模块内可以有自己的 Services（可选），但要避免跨模块直接调用 Manager，应该通过共享服务或事件解耦。

- 模块主要是通过继承 ManagerBase<T> 来实现业务逻辑，所有Manager都要继承 ManagerBase<T> 或 ManagerBase
- 在模块中业务验证错误，抛出 `BusinessException`

多模块共用的逻辑在`CoreMod`中实现，它作为核心模块被其他模块引用；除此之外，模块间不允许互相引用，以保持模块的独立性和可维护性。

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

## 开发流程

1. 定义层，即实体的定义，DbContext的处理，以及共享服务的编写(封装以便简化和复用)
2. 模块层，即Manager和DTO的生成和编写; 并检查是否添加新的服务注入等。
3. 服务层，即Controller的生成和编写
4. **执行构建验证**（必须步骤）：验证编译无错误
6. 判断是否要在执行迁移，如果添加或修改了实体，则执行`scripts/EFMigrations.ps1`脚本(要停止运行中的服务，以防dll锁定)，永远不要手动修改迁移文件。

**要优先使用MCP工具`Perigon`，生成或创建模块/Entity/DTO/Manager/Controller等内容。**

## 构建验证（每次修改后必须执行）

通过`dotnet build`，构建对应的服务项目，如`ApiService`或`AdminService`，验证编译无错误。

### 构建-修复循环
修改代码 → 构建 → 发现错误 → 修复 → 重新构建，直到无错误

MCP server config lives in [.vscode/mcp.json](../../../.vscode/mcp.json); use configured endpoints when invoking tools.


## 约定与规范

- 解决方案使用全局命名空间(`GlobalUsings.cs`)和中央包管理
- Manager之间不允许直接调用
- Module之间不会互相依赖
- Controller调用Manager方法，不直接访问 DbContext
- 不要面向接口编程。没有多个实现类的服务，不要为其创建接口。
- 遵守项目核心约定和模式（如Manager继承ManagerBase、控制器继承RestControllerBase）

### 代码复用

尽可能的将逻辑算法与业务实现(数据访问)分离，如果逻辑算法只在一个模块使用，则放在模块内的Services目录下，如果多个模块使用，则放在Share层，并且要封装通用的第三方库的调用，如缓存/邮件/消息队列等。

业务实体的类型转换，可直接在实体中实现。

在实际编写代码时，优先使用 `src/Perigon` 目录下已经提供好的基础方法、扩展方法和生成结果；只有当现有能力无法覆盖需求时，才新增局部实现。

### 多租户

架构支持多租户，也支持单租户，从AppHost的`appsettings.Development.json`配置中，可以知道当前是单租户还是多租户模式。如果是单租户模式，tenantId默认为Guid.Empty。

### 对象映射

优先使用`Perigon.AspNetCore.Utils.Extensions` 中的扩展方法`Merge/MapTo`进行映射。

### Aspire集成 

AppHost为Aspire项目.
优先使用Aspire生态提供的功能和中间件，可通过MCP工具搜索微软官方文档了解如何使用。

---

## 代码约定

- 使用文件作用域命名空间
- 使用主构造函数，并将依赖注入到构造函数参数中，减少属性注入和字段注入
- 使用集合表达式
- 优先使用异步编程，并传递 `CancellationToken`