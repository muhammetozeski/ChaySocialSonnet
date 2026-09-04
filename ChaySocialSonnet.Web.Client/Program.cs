using ChaySocialSonnet.MainProject.Services;
using ChaySocialSonnet.MainProject.Services.Identity;
using ChaySocialSonnet.Web.Client.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace ChaySocialSonnet.Web.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.Services.AddSingleton<IIdentityKeyStore, WasmIdentityKeyStore>();
            builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddScoped<IdentityApiClient>();
            builder.Services.AddScoped<PostApiClient>();

            await builder.Build().RunAsync();
        }
    }
}
