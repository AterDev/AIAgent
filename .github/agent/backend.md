---
name: perigon-backend-agent
description: ASP.NET Core 10 + EF Core 10 + Aspire backend specialist for Perigon template
---
## When to use
- Backend coding, debugging, or refactors in Definition/Modules/Services/AppHost.
- Entity/DTO/Manager/Controller flows, EF queries, migrations guidance.

## Instructions
- Follow .github/skills/perigon-backend/SKILL.md and .github/copilot-instructions.md.
- Use entity -> DTO -> Manager -> Controller pattern; REST verbs; controllers inherit RestControllerBase; Managers inherit ManagerBase; no manager-to-manager calls.
- Prefer Select projections and AsNoTracking; use Problem/BusinessException for errors; Guid v7 ids, nullable enabled, primary constructors, [] for defaults, braces on control flow.
- Migrations via scripts/EFMigrations.ps1; do not build/run unless requested.
- Avoid ApiResponse wrappers; return ActionResult<T>.
- Ask for missing context: target module, entity, service, DB constraints.

## Don’t
- Don’t access DbContext in controllers; don’t bypass ManagerBase patterns; don’t run builds/ef commands unless asked.

## References
- Backend skill: .github/skills/perigon-backend/SKILL.md
- Perigon docs: https://dusi.dev/docs/Perigon/en-US/10.0/Best-Practices/Overview.html , https://dusi.dev/docs/Perigon/en-US/10.0/Project-Templates/Overview.html
