---
name: engineer
description: End-to-end senior software engineering workflow for this repository. Use for implementation tasks that require understanding requirements, planning, coding, debugging, validation, cleanup, and delivery across backend, frontend, Aspire, Perigon, RAG, tests, or cross-module changes.
---

# Engineer

Use this skill when Codex needs to take a feature, fix, refactor, or investigation from request to verified delivery in this repository.

## Working Rules

- Start from project facts: inspect relevant files, existing patterns, generated clients, DTOs, services, controllers, and Aspire state before deciding.
- Do not guess APIs or behavior when local code, generated types, official docs, or project skills can answer it.
- Keep code readable and maintainable; add comments only when they clarify non-obvious logic.
- Prefer existing project abstractions and conventions over new abstractions.
- Prefer `dotnet`, `pwsh`, `pnpm`, Aspire CLI, and Perigon-related capabilities. Avoid Python unless a bundled skill/tool specifically requires it or it is clearly the best option.
- Build or run verification after a coherent set of changes, not after every tiny edit.
- Clean temporary artifacts before finishing.
- Do not revert unrelated user changes.

## Skill Routing

- Backend, API, EF Core, entities, DTOs, managers, controllers, migrations, service architecture: use `.agents/skills/backend/SKILL.md`.
- Angular pages/components/routes/forms/services/i18n/styles: use `.agents/skills/angular/SKILL.md`.
- Aspire lifecycle, runtime state, logs, traces, resource rebuild/restart, AppHost behavior: use `.agents/skills/aspire/SKILL.md` and related Aspire skills.
- Code review tasks: use `.agents/skills/code-review/SKILL.md`.
- .NET API surface inspection: use `.agents/skills/dotnet-inspect/SKILL.md` when available.

When multiple skills apply, read the narrowest relevant skill files before acting and combine their guidance. Project-local skills win over generic memory or assumptions.

## Workflow

### 1. Understand

- Read the user request and identify the concrete behavioral goal.
- Inspect the smallest relevant slice of the codebase first, then expand only as needed.
- Identify generated files, ownership boundaries, and runtime services affected by the change.
- If requirements conflict or the next action is risky without missing information, ask a concise question; otherwise proceed with a reasonable assumption.

### 2. Plan

- Make a short plan for non-trivial work.
- Separate backend, frontend, generated-client, configuration, and runtime validation steps when they interact.
- Choose verification based on the actual change and current Aspire state.

### 3. Implement

- Keep edits scoped to the request.
- Reuse existing types and generated DTOs instead of creating partial duplicate objects.
- For generated clients or services, prefer the repo’s generation workflow when required; do not hand-edit generated code unless the task explicitly calls for it or no generator is available.
- For repeated mechanical changes, consider a script, then remove temporary scripts afterward.

### 4. Validate

- Prefer Aspire runtime validation for Aspire-managed services:
  - `aspire ps --format Json --non-interactive`
  - `aspire describe --format Json --non-interactive`
  - `aspire resource <resource> rebuild --non-interactive`
  - `aspire logs <resource> --search "ERROR" --non-interactive`
  - `aspire otel logs <resource> --search "<term>" --format Json --non-interactive`
- For frontend changes running under Aspire, inspect `frontend` logs and hot-reload output instead of launching an unrelated build path.
- For backend services, rebuild the affected Aspire resource(s) rather than the whole AppHost unless the AppHost model changed.
- If tests are appropriate and available, run focused tests that cover the changed behavior.

### 5. Deliver

- Summarize the implemented behavior and the verification performed.
- Mention any residual risk, skipped verification, or runtime migration issue.
- If review is requested or the workflow expects review, hand off using Codex-native review/thread tools when available; otherwise report that the implementation is ready for review.
