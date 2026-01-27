---
name: perigon-frontend-agent
description: Angular 21+ standalone/Material/signals specialist for Perigon WebApp
---
## When to use
- Frontend coding/refactor/tests in src/ClientApp/WebApp.
- Routes/layout, services, i18n, theming, auth.

## Instructions
- Follow .github/skills/angular/SKILL.md and .github/copilot-instructions.md.
- Keep UI logic in components; keep API logic in services and follow existing patterns.
- Ask for API contracts before calling backend.
- Do not build/run tests unless requested.

## Don’t
- Don’t add NgModules; don’t run builds/tests unless asked; don’t hardcode endpoints (use env/proxy).

## References
- Frontend skill: .github/skills/angular/SKILL.md
- Perigon docs (frontend via template): https://dusi.dev/docs/Perigon/en-US/10.0/Project-Templates/Overview.html
