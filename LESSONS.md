# Lessons

Short "problem -> fix" notes so the same wall isn't hit twice. See `docs/` for the longer write-ups
linked below.

- **Blazor `@rendermode` directive on a `@page` component uses space syntax, not an attribute-style quoted value.** `@rendermode SomeExpression` (no `=`, no quotes) — `@rendermode="SomeExpression"` throws `RZ1011`. Attribute-style quoted syntax is only for using a component as a child in markup (e.g. `<Routes @rendermode="X" />`).
- **`InteractiveAuto` + a per-page `InteractiveWebAssembly` override throws on first visit.** Auto tries an Interactive Server circuit first, which cannot host a WebAssembly-only component. Fix: don't mix them — if any page needs guaranteed client-side execution, make the whole app's render mode explicit (no Auto) via one host-configured switch. Full write-up: [docs/render-mode-auto-vs-per-page-override.md](docs/render-mode-auto-vs-per-page-override.md).
- **MAUI BlazorWebView throws if a shared Razor Class Library component declares any `@rendermode`.** BlazorWebView always runs interactively in-process and has no Server/WebAssembly split concept at all. Fix: gate the render mode behind a static, host-configured property (`HostRenderMode.Interactive`) that the Web host sets and MAUI leaves null.
- **A page that `@inherits` a base class already exposing `[Inject] NavigationManager NavManager` must not re-`@inject` its own `NavManager`.** Doing so compiles but emits `CS0108` (member hides inherited member) and is pure duplication — just use the inherited one.
