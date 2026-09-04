using ChaySocialSonnet.MainProject.Backend;
using ChaySocialSonnet.Web.Backend;
using ChaySocialSonnet.Web.Components;

namespace ChaySocialSonnet
{
    public class Program
    {
        public static void Main(string[] args)
        {
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
