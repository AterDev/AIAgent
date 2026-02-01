---
name: backend
description: Asp.Net Core/Aspire 后端开发规范和最佳实践
---

## 何时使用

在编写后端项目时，如Aspire/Asp.Net Core WebAPI及其他与后端相关的内容。

## Perigon分层结构

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

- Shared constants live in src/Definition/Share/Constants; module-specific constants stay in their module/service.
- Extend AppConst via extension methods in Share/Constants (e.g., AppExtensions) rather than modifying base constants.

### 模块依赖层次（从下到上）

1. **Entity** → 定义数据模型
2. **EntityFramework** → 配置 DbContext，依赖 Entity
3. **Share + ServiceDefaults** → 共享工具和服务注册，依赖 EntityFramework
4. **Modules** → 业务逻辑和 DTO，依赖 Entity 和 Share
5. **Services** → API 控制器，依赖 Modules

**禁止**：
- ❌ Manager 之间直接调用（通过共享服务或事件解耦）
- ❌ Controller 绕过 Manager 直接访问 DbContext
- ❌ Entity 包含业务逻辑（仅数据模型和验证注解）
- ❌ 不要面向接口编程。没有多个实现类的服务，不要为其创建接口。

---

## 开发流程

1. 先处理定义层，即实体的定义，DbContext的处理，以及共享服务的编写
2. 然后处理模块层，即Manager和DTO的编写
3. 最后处理服务层，即Controller的编写
4. 检查项目依赖关系，检查错误，确保没有违反分层原则
5. 没有错误，添加或修改了实体，必须通过执行`scripts/EFMigrations.ps1`脚本，生成迁移文件。

**要优先使用MCP工具`Perigon`，生成或创建模块/Entity/DTO/Manager/Controller等内容。**

MCP server config lives in [.mcp.json](.mcp.json) and [.vscode/mcp.json](.vscode/mcp.json); use configured endpoints when invoking tools.

---

## 开发规范

### 错误处理
- 业务错误：抛出 `BusinessException`
- 第三方服务调用：保留在`Share/Services`中

---

### DTO（数据传输对象）

通过MCP工具生成Dto，然后以此为模板再根据实际需求调整。

### Manager

通过MCP工具生成Manager，然后以此为模板再根据实际需求调整。

如果Manager没有绑定特定的实体，则继承`ManagerBase.cs`的其他基类。

EF Core 查询，要使用方法调用，而不是使用 LINQ 查询语法。

###  对象映射

优先使用`Perigon.AspNetCore.Utils.Extensions` 中的扩展方法`Merge/MapTo`进行映射。

### 控制器（Controllers）

通过MCP工具生成Controller，然后以此为模板再根据实际需求调整。

如果Controller没有绑定特定的Manager，则继承`RestControllerBase`的其他基类。

✗ **避免**：
- 直接访问 DbContext
- 实现业务逻辑
- ApiResponse 包装器（使用标准响应）

### 返回值
- **成功**：`ActionResult<T>` 直接返回模型
- **错误**：使用 `Problem()` 或 `NotFound()`
- **参数绑定**：有歧义时使用显式特性 `[FromBody]` / `[FromQuery]` / `[FromRoute]`

### Aspire / AppHost

可参考微软官方文档以实现相关的需求。
优先使用Aspire生态提供的功能和中间件。

---

## 代码约定

### C# 14 语言特性
- 使用文件作用域命名空间
- 使用主构造函数
- 使用集合表达式

### 异步编程
- **全程异步**：使用 `async/await`
- **传递 Token**：从 Controller 向 Manager/数据访问传递 `CancellationToken`

### 依赖注入
- 使用构造函数注入（Constructor Injection）
- ✗ 避免服务定位器模式

### 验证和业务规则
- **API 边界**：验证输入参数
- **业务逻辑**：验证规则保留在 Manager 中