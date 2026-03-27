---
name: perigon
description: "使用 Perigon CLI 进行解决方案脚手架、模块/服务添加、OpenAPI 客户端生成、MCP Server 配置与启动、Studio 面板管理、模块打包与安装。USE FOR: perigon new, perigon add module, perigon add service, perigon generate request, perigon mcp config/start, perigon studio, perigon pack, perigon install. 
---

# Perigon Skill

This repository uses `Perigon.CLI` for code generation, and MCP integration.

## CLI command reference

| Task | Command |
|---|---|
| Create a new solution | `perigon new <name>` |
| Launch Studio | `perigon studio` |
| Update Studio | `perigon studio update` |
| Add a module | `perigon add module <ModuleName>` |
| Add a service | `perigon add service <ServiceName>` |
| Generate request client | `perigon generate request <path|url> <outputPath>` |
| Generate models only | `perigon generate request <path|url> <outputPath> -m` |
| Show MCP config | `perigon mcp config` |
| Start MCP server | `perigon mcp start` |
| Pack a module | `perigon pack <ModuleName> <ServiceName>` |
| Install a module package | `perigon install <PackagePath> <ServiceName>` |
| List official modules | `perigon install -l` |
| Show command help | `perigon -h` or `perigon <command> -h` |

## Key workflows

### Generating request clients from OpenAPI

Use `perigon generate request` when the task is to generate typed request services or models from a Swagger/OpenAPI document.

```bash
perigon generate request ./openapi.json ./src/services -t angular
```

Supported types: `angular` (default), `csharp`, `axios`.

In this repository, the preferred frontend `outputPath` is the absolute path of `src/ClientApp/WebApp/src/app`.

## Important rules

- **Use Perigon for scaffolding and code generation**, not for build/test/run.
- **Prefer generated code over hand-written boilerplate** for modules, services, and request clients.
- **For frontend API clients, prefer `perigon generate request`** and keep generated contracts consistent with backend OpenAPI definitions.
- **Do not manually edit generated request contracts unless necessary**; regenerate when the backend changes.
- **Check help before guessing options** with `perigon <command> -h`.

## Notes

- `perigon new` and `perigon studio` are interactive solution-creation flows.
- `perigon add module <ModuleName>` allows omitting the `Mod` suffix.
- `perigon pack <ModuleName> <ServiceName>` expects the module name with the `Mod` suffix.
- `perigon install` accepts either a local zip path or an official package name such as `Perigon.SystemMod`.
