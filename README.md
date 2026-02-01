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

* `src/Ater/Perigon.AspNetCore`: 基础类库，提供基础帮助类。
* `src/Definition/ServiceDefaults`: 是提供基础的服务注入的项目。
* `src/Definition/Entity`: 包含所有的实体模型，按模块目录组织。
* `src/Definition/EntityFramework`: 基于Entity Framework Core的数据库上下文
* `src/Modules/`: 包含各个模块的程序集，主要用于业务逻辑实现
	* `src/Modules/XXXMod/Managers`: 各模块下，实际实现业务逻辑的目录
	* `src/Modules/XXXMod/Models`: 各模块下，Dto模型定义，按实体目录组织
* `src/Services/ApiService`: 是接口服务项目，基于ASP.NET Core Web API
* `src/Services/AdminService`: 后台管理服务接口项目

## 项目运行

项目基于`Aspire`，直接运行`AppHost`项目即可启动所有服务。

## 文档

- [快速入门](https://dusi.dev/docs/Perigon/zh-CN/10.0/%E5%BF%AB%E9%80%9F%E5%85%A5%E9%97%A8.html)
- [项目模板](https://dusi.dev/docs/Perigon/zh-CN/10.0/%E9%A1%B9%E7%9B%AE%E6%A8%A1%E6%9D%BF/%E6%A6%82%E8%BF%B0.html)
- [开发规范](https://dusi.dev/docs/Perigon/zh-CN/10.0/%E6%9C%80%E4%BD%B3%E5%AE%9E%E8%B7%B5/%E5%BC%80%E5%8F%91%E8%A7%84%E8%8C%83%E4%B8%8E%E7%BA%A6%E5%AE%9A.html)


完整文档请阅读[Perigon官方文档](https://dusi.dev/docs/Perigon.html)。