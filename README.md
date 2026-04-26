# AIAgent 平台

![NuGet Version](https://img.shields.io/nuget/v/Perigon.templates?style=flat)

一个开箱即用的 AI Agent 管理平台，提供模型管理、Agent 配置、工作流编排、知识库管理和 MCP 工具集成等完整功能。

## ✨ 新增功能 (2026-02)

### 🔧 AI 能力管理前端页面

- **模型在线调试** (`/model-debug`) - 实时测试 AI 模型，查看响应和 Token 使用情况
- **Agent 配置与调试** (`/agent-debug`) - 配置和调试 AI Agent，查看工具调用详情
- **工作流编排监控** (`/workflow-monitor`) - 实时监控工作流执行，查看步骤详情

详细说明请查看 [实现总结文档](docs/实现方案-前端管理页面/2-实现总结.md)

---

## 项目说明

`Perigon.templates`项目模板的使用提供文档支持。

## 根目录

- docs: 项目文档存储目录
- scripts： 项目脚本文件目录
- src：项目代码目录
- test：测试项目目录
- .config：配置文件目录

## 代码目录src

* `src/Perigon/Perigon.AspNetCore`: 基础类库，提供基础帮助类。
* `src/Definition/ServiceDefaults`: 是提供基础的服务注入的项目。
* `src/Definition/Entity`: 包含所有的实体模型，按模块目录组织。
* `src/Definition/EntityFramework`: 基于Entity Framework Core的数据库上下文
* `src/Modules/`: 包含各个模块的程序集，主要用于业务逻辑实现
	* `src/Modules/XXXMod/Managers`: 各模块下，实际实现业务逻辑的目录
	* `src/Modules/XXXMod/Models`: 各模块下，Dto模型定义，按实体目录组织
* `src/Services/ApiService`: 是接口服务项目，基于ASP.NET Core Web API
* `src/Services/AdminService`: 后台管理服务接口项目

在实际编写代码时，优先复用 Perigon 目录和既有模块中已经提供好的基础类、扩展方法和生成结果，只有在现有能力无法覆盖时，再补充新的实现。

## 项目运行

项目基于`Aspire`，直接运行`AppHost`项目即可启动所有服务。

## Compose 发布与运行

项目已经提供基于 Aspire 的 compose 发布脚本：[scripts/Compose.ps1](scripts/Compose.ps1)。

默认情况下，脚本不会在仓库根目录生成 `docker-compose.yml`。它会先调用 `aspire publish`，然后把 compose 产物写入 `artifacts/compose/` 目录：

- `artifacts/compose/docker-compose.yaml`：Aspire 生成的基础 compose 文件
- `artifacts/compose/docker-compose.override.yaml`：脚本补充的端口映射和本地运行覆盖项
- `artifacts/compose/.env`：脚本生成的镜像名、密码和端口变量

脚本不会再生成自定义 `Dockerfile.*`。`Build` 阶段改为直接调用 `.NET SDK` 的 `PublishContainer`，由 SDK 生成本地镜像。

常用命令：

```powershell
# 仅生成 Aspire compose 产物和本地覆盖文件
.\scripts\Compose.ps1 -Action Generate

# 生成产物并构建本地镜像
.\scripts\Compose.ps1 -Action Build

# 生成产物并启动 compose 环境
.\scripts\Compose.ps1 -Action Up

# 完整执行：生成、构建、启动
.\scripts\Compose.ps1 -Action All

# 查看状态、日志和停止环境
.\scripts\Compose.ps1 -Action Ps
.\scripts\Compose.ps1 -Action Logs
.\scripts\Compose.ps1 -Action Down
```

说明：

- 默认运行时为 `podman`，可通过 `-Runtime docker` 切换为 Docker。
- 默认输出目录为 `artifacts/compose`，可通过 `-OutputPath` 自定义。
- `Generate`、`Build`、`Up`、`All` 都会刷新 `artifacts/compose/docker-compose.yaml`，因为脚本每次都会重新执行一次 `aspire publish`，确保 compose 产物与当前 AppHost 配置一致。
- `Build` 阶段使用 `dotnet publish /t:PublishContainer` 构建本地镜像，因此 NuGet 包优先复用主机缓存，而不是依赖容器内的 restore 缓存层。
- 为避免本机 `C:` 盘缓存和临时目录不足，脚本会把容器发布相关的 NuGet/Temp 目录重定向到仓库下的 `D:\codes\AIAgent\.cache\nuget`。
- 当前验证通过的对外端口包括：`15001`（AdminService）、`15002`（ApiService）、`15003`（FileProcessorService）。

## 文档

- [快速入门](https://dusi.dev/docs/Perigon/zh-CN/10.0/%E5%BF%AB%E9%80%9F%E5%85%A5%E9%97%A8.html)
- [项目模板](https://dusi.dev/docs/Perigon/zh-CN/10.0/%E9%A1%B9%E7%9B%AE%E6%A8%A1%E6%9D%BF/%E6%A6%82%E8%BF%B0.html)
- [开发规范](https://dusi.dev/docs/Perigon/zh-CN/10.0/%E6%9C%80%E4%BD%B3%E5%AE%9E%E8%B7%B5/%E5%BC%80%E5%8F%91%E8%A7%84%E8%8C%83%E4%B8%8E%E7%BA%A6%E5%AE%9A.html)


完整文档请阅读[Perigon官方文档](https://dusi.dev/docs/Perigon.html)。