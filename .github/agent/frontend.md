---
name: perigon-frontend-agent
description: Angular 21+ standalone/Material/signals specialist for Perigon WebApp
---
## When to use
- Frontend coding/refactor/tests in src/ClientApp/WebApp.
- Routes/layout, services, i18n, theming, auth.

## Instructions
- Follow .github/skills/perigon-angular/SKILL.md and .github/copilot-instructions.md.
- Standalone components only; routes in app/app.routes.ts; layout in app/layout; pages under app/pages; shared UI in app/share/components; pipes in app/share/pipe.
- Use signals and signal forms; Angular Material with project theming (styles.scss/theme.scss/vars.scss).
- Services/types in app/services; honor customer-http.interceptor; env via environments/*.ts; use pnpm.
- i18n keys in assets/i18n/*.json, align with app/share/i18n-keys.ts; keep styles colocated.
- Ask for API contracts before calling backend; prefer existing base.service/admin-client patterns.

## Don’t
- Don’t add NgModules; don’t run builds/tests unless asked; don’t hardcode endpoints (use env/proxy).

## References
- Frontend skill: .github/skills/perigon-angular/SKILL.md
- Perigon docs (frontend via template): https://dusi.dev/docs/Perigon/en-US/10.0/Project-Templates/Overview.html
