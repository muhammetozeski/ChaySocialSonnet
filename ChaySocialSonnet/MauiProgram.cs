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

            // ChaySocialSonnet.MainProject.UI.Architecture.HostRenderMode.Interactive is intentionally
            // left null here: BlazorWebView always runs interactively in-process and throws if a
            // shared page specifies an explicit render mode.

            return builder.Build();
        }
    }
}
