using ChaySocialSonnet.MainProject.Backend;
using ChaySocialSonnet.MainProject.UI.Architecture;
using ChaySocialSonnet.Web.Backend;
using ChaySocialSonnet.Web.Components;
using Microsoft.AspNetCore.Components.Web;

namespace ChaySocialSonnet
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Every page can touch a private key, so the whole app must always run client-side, even on
            // the first request — Interactive Server/Auto would execute component code on this server
            // process at least once. See HostRenderMode's own summary for the full reasoning.
            HostRenderMode.Interactive = RenderMode.InteractiveWebAssembly;

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddInteractiveWebAssemblyComponents();

            // Local, in-memory backend for now — the developer's own PC is "the server". Each of these
            // is registered behind its MainProject interface, so swapping in a Firebase-backed
            // implementation later is a one-line change here, not a change to any caller.
            builder.Services.AddSingleton<IIdentityRegistry, LocalIdentityRegistry>();
            builder.Services.AddSingleton<IPostStore, LocalPostStore>();
            builder.Services.AddSingleton<IMessageRelay, LocalMessageRelay>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            app.UseHttpsRedirection();

            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode()
                .AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(
                    typeof(ChaySocialSonnet.MainProject.UI.Routes).Assembly,
                    typeof(ChaySocialSonnet.Web.Client._Imports).Assembly);

            app.Run();
        }
    }
}
