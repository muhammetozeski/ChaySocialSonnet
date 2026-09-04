using ChaySocialSonnet.MainProject.Services.Identity;
using Microsoft.Maui.Storage;
using System.Text.Json;

namespace ChaySocialSonnet.Services
{
    /// <summary>
    /// Persists the <see cref="ChayIdentity"/> in OS-native secure storage (Android Keystore / Windows
    /// DPAPI, via <see cref="SecureStorage"/>) so it survives app restarts on this device only.
    /// </summary>
    public sealed class MauiIdentityKeyStore : IIdentityKeyStore
    {
        const string StorageKey = "chay_identity";

        public Task SaveAsync(ChayIdentity identity) =>
            SecureStorage.Default.SetAsync(StorageKey, JsonSerializer.Serialize(identity));

        public async Task<ChayIdentity?> LoadAsync()
        {
            string? json = await SecureStorage.Default.GetAsync(StorageKey);
            return string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<ChayIdentity>(json);
        }

        public Task ClearAsync()
        {
            SecureStorage.Default.Remove(StorageKey);
            return Task.CompletedTask;
        }
    }
}
