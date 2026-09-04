using Microsoft.AspNetCore.Components;

namespace ChaySocialSonnet.MainProject.UI.Architecture
{
    /// <summary>
    /// Host-configured render mode for the app's whole interactive tree. Every page in this project can
    /// touch a private key or other secret that must never run on the server, so the app never uses
    /// Interactive Server or Auto (both would execute component code server-side, at least for the
    /// first render) — it always runs client-side. Each host app sets this once at startup:
    /// - The Blazor Web App host (<c>ChaySocialSonnet.Web</c>) sets it to
    ///   <see cref="RenderMode.InteractiveWebAssembly"/> in its <c>App.razor</c> root, so the entire app
    ///   executes in the browser, even on the very first request, and never touches the server process.
    /// - MAUI (<c>ChaySocialSonnet</c>, via BlazorWebView) leaves it null — MAUI's hosting model always
    ///   runs interactively in-process and throws if a shared component specifies any render mode.
    /// Reference this via <c>@rendermode="HostRenderMode.Interactive"</c> instead of a literal render
    /// mode, so the same .razor file compiles and behaves correctly under both hosts.
    /// </summary>
    public static class HostRenderMode
    {
        public static IComponentRenderMode? Interactive { get; set; }
    }
}
