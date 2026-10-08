# Perigon CLI、MCP 与代码生成

## 先检查实时能力

先在解决方案根目录运行 `perigon -h` 和目标子命令的 `-h`。CLI 及 MCP 工具可能随版本变化；不要从旧文档猜测参数，也不要固化实时帮助中不存在的命令。

Perigon 用于创建、安装和生成，不代替 `dotnet build/test`、前端构建或 Aspire 生命周期管理。

## 常用命令

| 任务 | 命令 |
|---|---|
| 创建解决方案 | `perigon new <name>` |
| 对比并升级项目基础架构 | `perigon update` |
| 添加模块/服务 | `perigon add module <name>` / `perigon add service <name>` |
| 输出实体建模规则 | `perigon generate entity`；只输出规则，不创建文件 |
| Standard 生成 DTO/Manager/Controller | `perigon generate dto|manager|controller ...` |
| 从 OpenAPI 生成客户端 | `perigon generate request <path|url> <output> -t angular|axios|csharp` |
| 安装/打包模块 | `perigon module install|pack ...` |
| 初始化 Agent 能力 | `perigon agent init` |
| 启动 stdio MCP | `perigon agent mcp` |
| 启动 Studio 与 HTTP MCP | `perigon studio` |

仅当当前帮助确认支持时才使用其他命令或选项。`-f/--force` 会覆盖生成文件，执行前必须确认目标和 diff。

## 升级现有项目

在解决方案根目录的交互式终端运行 `perigon update`。它会更新本机的 `Perigon.templates`、按当前模板类型和前端模式生成临时项目，然后只对比受管基础架构：`src/Perigon`、`Definition/ServiceDefault(s)`、`Definition/Share`、`Definition/EntityFramework`、`scripts/*.ps1` 与 `.agent/.agents`。

左栏按文件名列出差异；方向键移动，空格选择，右栏查看完整路径和内容差异，Enter 应用，Esc 取消。更新不会删除仅存在于当前项目的文件，并排除 EF migrations、模板规定的 DbContext 以及 `Perigon.AspNetCore/Constants/WebConst.cs`。应用后 CLI 自动运行 `dotnet build`；构建失败时根据输出修复，不能把文件复制完成视为升级成功。

## 内置生成器边界

DTO、Manager、Controller 内置生成器适用于 Standard；`.config/perigon.config.toml` 中 `isAOT = true` 的 MiniApi 不支持这组生成器。MiniApi 应采用现有 Minimal API 结构或经验证的自定义生成任务。

生成依赖链为 Controller → Manager → DTO；生成 Controller 会补齐依赖。生成前确认实体路径、模块、目标 Service 和现有文件：

- Manager 位于模块并承载业务逻辑；Controller 位于目标 Service。
- DTO 默认按 Item、Detail、Filter、Add、Update 用途裁剪属性。生成后重点检查导航属性、敏感字段、可空性、部分更新语义、长文本和二进制字段。
- 模块 DTO 放在 `Models/{Entity}Dtos`，一个类型一个文件，数据传输类型以 `Dto` 结尾。
- Controller 生成的 CRUD、权限和 OwnedIds 逻辑只是起点，必须审查真实授权、租户和业务规则。

## OpenAPI 客户端

`perigon generate request` 支持：

- `-m/--only-model`：只生成模型。
- `-c/--cover-base-service`：覆盖已有基础服务；默认应保留用户定制的基础服务。
- C# 客户端使用相对 URI 时，`HttpClient.BaseAddress` 必须以 `/` 结尾。
- 204、205、304 与 HEAD 应生成无内容返回；OpenAPI tag 会影响服务名。

生成后检查输入契约、输出目录、服务命名、OperationId、枚举描述和基础服务 diff，再构建对应客户端。后端公开契约变化时，顺序为：更新接口 → 生成/获取 OpenAPI → 生成客户端 → 构建客户端。

## MCP 模式

- `perigon agent init` 可初始化 MCP 或 Skills；MCP 配置通常写入 `.vscode/mcp.json`。
- `perigon agent mcp` 是面向 IDE/代码 Agent 的 stdio Server，通过 roots 定位项目。stdout 只能承载 MCP 协议，普通日志不能写入 stdout。
- 新增自定义 MCP 工具后重启客户端中的 MCP Server，才能重新发现工具。
- MCP 会直接修改模块或代码文件；调用前确认目标路径，调用后检查 `git diff`。

## Skills 与自定义生成

Studio 不再提供提示词、Razor 模板、自定义生成任务或 MCP 工具配置页面。需要项目专用生成时，通过当前 `perigon` skill 向 AI 描述目标，并优先调用现有 CLI/MCP；没有内置生成器的场景由 AI 按项目架构直接创建代码。保留可复用的约束、命令和审查规则在 skill/reference 中，生成后仍执行常规代码审查、测试和构建。

## 生成后检查

1. 检查新增/覆盖文件数量、目录、命名空间和目标服务。
2. 检查 DTO、Manager、Controller/Endpoint 的职责边界。
3. 检查授权、租户、输入验证、分页/查询规模和敏感字段。
4. 实体变化按模板处理迁移；公开契约变化同步 OpenAPI 与客户端。
5. 运行受影响项目的构建和测试；MiniApi 额外验证 NativeAOT publish。
