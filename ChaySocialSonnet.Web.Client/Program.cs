using ChaySocialSonnet.MainProject.Services;
using ChaySocialSonnet.MainProject.Services.Identity;
using ChaySocialSonnet.Web.Client.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace ChaySocialSonnet.Web.Client
{
    internal class Program
    {
        /// <summary> Caps how long a single server round trip (register/challenge/verify/posts) can hang before the calling page's AsyncOperationState reports it as a failure instead of leaving the UI stuck indefinitely. </summary>
        static readonly TimeSpan ApiRequestTimeout = TimeSpan.FromSeconds(15);

        static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.Services.AddSingleton<IIdentityKeyStore, WasmIdentityKeyStore>();
            builder.Services.AddScoped(_ => new HttpClient
            {
                BaseAddress = new Uri(builder.HostEnvironment.BaseAddress),
                Timeout = ApiRequestTimeout
            });
            builder.Services.AddScoped<IdentityApiClient>();
            builder.Services.AddScoped<PostApiClient>();

            await builder.Build().RunAsync();
        }
    }
}
