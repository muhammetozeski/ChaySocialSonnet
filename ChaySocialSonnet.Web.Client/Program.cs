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

            await builder.Build().RunAsync();
        }
    }
}
