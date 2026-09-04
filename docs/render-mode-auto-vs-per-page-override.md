# Blazor Web App: InteractiveAuto conflicts with a per-page InteractiveWebAssembly override

## Problem

`ChaySocialSonnet.Web`'s `App.razor` originally set `<Routes @rendermode="InteractiveAuto" />` (the
default Blazor Web App template setting), while individual pages that touch a private key
(`Login.razor`) declared their own `@rendermode HostRenderMode.SecuritySensitive` (resolving to
`RenderMode.InteractiveWebAssembly`) to guarantee they never execute on the server.

## Symptom

On a fresh browser session (no cached WASM preference), navigating to `/login` threw:

```
warn: Microsoft.AspNetCore.Components.Server.Circuits.RemoteRenderer[100]
      Unhandled exception rendering component: Cannot create a component of type
      'ChaySocialSonnet.MainProject.UI.Pages.Login' because its render mode
      'Microsoft.AspNetCore.Components.Web.InteractiveWebAssemblyRenderMode' is not supported by
      interactive server-side rendering.
fail: Microsoft.AspNetCore.Components.Server.Circuits.CircuitHost[111]
      Unhandled exception in circuit '...'.
```

The page still ended up rendering and interactive (Blazor's Auto-mode client bootstrapper caught the
failed Server attempt and fell back to downloading and booting the WebAssembly runtime), but the
server logged a genuine unhandled exception and tore down a circuit on every first visit. This matches
a known upstream issue: Interactive Auto does not cleanly switch from Server to WebAssembly when a
nested routed page declares its own, different render mode (see
[dotnet/aspnetcore#53799](https://github.com/dotnet/aspnetcore/issues/53799)).

## Root cause

`InteractiveAuto` on the app root tries an Interactive Server circuit first (fast first paint, no WASM
download wait). That Server circuit is fundamentally unable to instantiate ANY WebAssembly-only
component — there's no WASM runtime on the server. The framework's per-page render mode override is
not resolved early enough to prevent Auto from attempting the Server circuit for the whole route
first, so the conflict throws before the fallback to WebAssembly kicks in.

## Solution

Since every page in this app can touch a private key or other secret, there is no page that actually
benefits from Auto's "fast Server paint, upgrade later" trade-off — the whole app should just always
run client-side. Fix: stop using `InteractiveAuto` at all. `HostRenderMode.Interactive`
(`MainProject/UI/Architecture/HostRenderMode.cs`) is set once, app-wide, in `ChaySocialSonnet.Web`'s
`Program.cs`:

```csharp
HostRenderMode.Interactive = RenderMode.InteractiveWebAssembly;
```

and referenced directly on the app root in `App.razor`:

```razor
<HeadOutlet @rendermode="ChaySocialSonnet.MainProject.UI.Architecture.HostRenderMode.Interactive" />
...
<Routes @rendermode="ChaySocialSonnet.MainProject.UI.Architecture.HostRenderMode.Interactive" />
```

Individual pages (`Splash.razor`, `Login.razor`) no longer need their own `@rendermode` at all — they
inherit the app-wide WebAssembly mode. `HostRenderMode.Interactive` stays null for the MAUI host
(`MauiProgram.cs`), because BlazorWebView always runs interactively in-process and throws if a shared
component specifies any render mode (a separate, unrelated constraint — see that file's comment).
