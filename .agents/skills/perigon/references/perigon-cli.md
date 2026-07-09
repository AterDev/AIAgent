# Perigon CLI Reference

This reference collects the CLI / agent guidance for the Perigon skill.

## Scope

Use this reference whenever the task involves:

- scaffolding a new solution or adding modules/services;
- generating DTOs, managers, controllers, and request clients;
- installing, listing, or packing module packages;
- initializing agent setup, MCP / Skills integration, or Studio;
- checking CLI help before guessing options.

## Core command reference

| Task | Command |
|---|---|
| Create a new solution | `perigon new <name>` |
| Add a module | `perigon add module <ModuleName>` |
| Add a service | `perigon add service <ServiceName>` |
| Generate DTO from an entity | `perigon generate dto <EntityPath>` |
| Generate manager from an entity | `perigon generate manager <EntityPath>` |
| Generate controller from an entity and target service | `perigon generate controller <EntityPath> <ServicePath|ServiceName>` |
| Generate typed request client/models from OpenAPI | `perigon generate request <path|url> <outputPath>` |
| Generate only models from OpenAPI | `perigon generate request <path|url> <outputPath> -m` |
| List official module packages | `perigon module list` |
| Install a module package | `perigon module install <PackagePath|OfficialName> <ServiceName>` |
| Pack a module into a zip | `perigon module pack <ModuleName> <ServiceName>` |
| Initialize agent setup | `perigon agent init` |
| Start agent MCP server | `perigon agent mcp start` |
| Launch Studio | `perigon studio` |
| Update Studio | `perigon studio update` |
| Show command help | `perigon -h` or `perigon <command> -h` |

## Recommended workflows

### 1. Solution and project scaffolding

- Prefer `perigon new <name>` for new solution creation.
- Prefer `perigon add module <ModuleName>` and `perigon add service <ServiceName>` to extend the current template.
- `perigon add module <ModuleName>` allows omitting the `Mod` suffix.

### 2. Code generation from entities and services

- Use `perigon generate dto <EntityPath>` to create DTO classes from an entity file.
- Use `perigon generate manager <EntityPath>` to create manager classes.
- Use `perigon generate controller <EntityPath> <ServicePath|ServiceName>` for controller generation.
- Add `-f` / `--force` when you want to overwrite existing generated files.

### 3. OpenAPI / frontend request generation

- Use `perigon generate request` when the task is to produce typed request services or models from Swagger / OpenAPI.
- Supported types: `angular`, `csharp`, `axios`.
- Use `-m` / `--only-model` when only models are needed.

### 4. Module package workflows

- Use `perigon module list` to inspect official modules.
- Use `perigon module install <PackagePath|OfficialName> <ServiceName>` to install a module into the current project.
- Use `perigon module pack <ModuleName> <ServiceName>` to package a module for distribution.

### 5. Agent / MCP / Skills workflows

- Use `perigon agent init` to choose the setup path.
- Choose `MCP` to keep the existing behavior: write MCP stdio config into `.vscode/mcp.json`.
- Choose `Skills` to add agents and skills.
- Use `perigon agent mcp start` to launch the agent MCP server.
- Use `perigon studio` to open Studio, and `perigon studio update` to refresh it.

## Project structure to keep in mind

The current template is organized as:

- `src/AppHost/` for Aspire orchestration and resource setup;
- `src/Definition/` for entities, EF Core context, shared defaults, and shared models;
- `src/Services/` for service hosts such as `ApiService` and `AdminService`;
- `src/ClientApp/WebApp/` for the frontend application;
- `tests/`, `docs/`, and `scripts/` for validation, documentation, and automation.

## Rules

- Use Perigon for scaffolding and code generation; do not use it for build / test / run tasks.
- Prefer generated code over manually re-creating boilerplate.
- For frontend API clients, keep generated contracts aligned with the backend OpenAPI contract.
- Check help with `perigon -h` or `perigon <command> -h` before making assumptions.
