# Angular Reference

This reference belongs to the Perigon skill and covers Angular 21+ frontend work for this template.

## When to use

Use this reference when the task involves:

- building CRUD pages, forms, routes, shared components, and Material-based layouts;
- generating typed request clients from backend OpenAPI through Perigon;
- implementing or reviewing frontend changes in a Perigon-based project.

## Project structure

```sh
src/
  ├── main.ts
  ├── app/
  │   ├── app.config.ts
  │   ├── app.routes.ts
  │   ├── layout/
  │   ├── pages/
  │   ├── share/
  │   │   ├── components/
  │   │   ├── pipe/
  │   │   ├── auth.guard.ts
  │   │   ├── custom-paginator-intl.ts
  │   │   └── i18n-keys.ts
  │   └── services/
  ├── assets/i18n/
  ├── environments/
  ├── styles/
  └── proxy.conf.json
```

## Core rules

- Prefer standalone components and Angular Material.
- Prefer signals and typed forms.
- Use i18n keys instead of hard-coded user-facing strings.
- Keep generated request contracts untouched and regenerate them when the backend changes.
- Avoid inline styles and prefer shared styles / Material tokens.
- Prefer `async pipe` or signals over imperative subscriptions where possible.
- Avoid calling functions in templates and keep the UI logic structured and typed.

## Recommended workflow

1. Generate or refresh request clients from backend OpenAPI through Perigon.
2. Create the page or component structure.
3. Configure route/menu and shared UI pieces.
4. Implement the TypeScript / HTML / SCSS logic and validate the interaction states.
5. Run `pnpm build` (and optionally `pnpm start`) to verify the result.

## UI / UX and component guidance

- Prefer standalone components and Angular Material, and follow the existing theme and spacing system instead of introducing new visual styles.
- Use signals and typed forms whenever possible. Prefer `async pipe` or signals over subscription-heavy imperative patterns.
- Keep user-visible strings in i18n rather than hard-coding text. Use the shared i18n keys structure and `translate` pipe.
- For page structure, follow a consistent enterprise-admin pattern: title / toolbar, filter or content area, main result area, and clear primary actions.
- For list pages, keep the main actions prominent and place secondary actions in menus when appropriate.
- For form pages, use a compact single-column layout on small screens and a two-column layout on wider screens when the form is complex.
- For tables, provide clear empty, loading, error, and disabled states. Avoid designing pages without state handling.
- Prefer Material components such as `mat-table`, `mat-paginator`, `mat-dialog`, `mat-snackbar`, `mat-select`, `mat-slide-toggle`, and form fields rather than custom controls.
- Use the existing style hierarchy from the theme, vars, and component styles; do not add inline styles or override Material internals with `::ng-deep` unless no public API is available.

## Form and layout conventions

- Use reactive forms and typed form controls. Prefer `formControl` + getters over repeated string-based form access.
- Ensure validation feedback is shown near the relevant control and keep submit states explicit.
- Avoid horizontal overflow in general page layouts. Internal tables and long code/text blocks are the main exceptions.
- Use bootstrap-style flex utilities and spacing helpers for layout rather than relying on ad-hoc container/row/column structures.

## UX and validation checklist

- Check layout density, empty / loading / error states, and responsive behavior.
- Prefer Material components and existing theme styles over custom visual overrides.
- Keep user-visible strings in i18n and avoid hard-coded text.
- Review the page structure for clarity, spacing, and tool-like enterprise-backend aesthetics.

## Verification

- Run `pnpm build` after frontend changes.
- Prefer `pnpm start` for live validation when the page is non-trivial.
