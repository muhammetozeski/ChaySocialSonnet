using ChaySocialSonnet.MainProject.Services;
using ChaySocialSonnet.MainProject.Services.Identity;
using ChaySocialSonnet.Services;
using Microsoft.Extensions.Logging;

namespace ChaySocialSonnet
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddSingleton<IIdentityKeyStore, MauiIdentityKeyStore>();

            // No server to talk to yet from the native app (ChaySocialSonnet.Web is only reachable over
            // the network once mobile builds actually ship) — this HttpClient has no BaseAddress until
            // that's wired up. Registered anyway so shared pages that inject IdentityApiClient still
            // resolve correctly here.
            builder.Services.AddSingleton<HttpClient>();
            builder.Services.AddSingleton<IdentityApiClient>();
            builder.Services.AddSingleton<PostApiClient>();
            builder.Services.AddSingleton<MessagesApiClient>();

            // ChaySocialSonnet.MainProject.UI.Architecture.HostRenderMode.Interactive is intentionally
            // left null here: BlazorWebView always runs interactively in-process and throws if a
            // shared page specifies an explicit render mode.

            return builder.Build();
        }
    }
}
