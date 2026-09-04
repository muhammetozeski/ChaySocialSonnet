using ChaySocialSonnet.MainProject.Services.Identity;
using Microsoft.JSInterop;
using System.Text.Json;

namespace ChaySocialSonnet.Web.Client.Services
{
    /// <summary>
    /// Persists the <see cref="ChayIdentity"/> in the browser's localStorage so it survives page
    /// reloads on this device only. Uses Blazor's built-in JS interop to call the browser's own
    /// localStorage global directly — no custom JavaScript is authored anywhere for this.
    /// </summary>
    public sealed class WasmIdentityKeyStore(IJSRuntime jsRuntime) : IIdentityKeyStore
    {
        const string StorageKey = "chay_identity";

        public async Task SaveAsync(ChayIdentity identity) =>
            await jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, JsonSerializer.Serialize(identity));

        public async Task<ChayIdentity?> LoadAsync()
        {
            string? json = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            return string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<ChayIdentity>(json);
        }

        public async Task ClearAsync() =>
            await jsRuntime.InvokeVoidAsync("localStorage.removeItem", StorageKey);
    }
}
