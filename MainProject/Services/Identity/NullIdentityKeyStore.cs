namespace ChaySocialSonnet.MainProject.Services.Identity
{
    /// <summary>
    /// No-op <see cref="IIdentityKeyStore"/> used where there is no real on-device storage to reach —
    /// specifically the Blazor Web App's server-side static prerender pass, which must never touch
    /// device-local storage (that would mean the server holding or inspecting key material). Always
    /// reports "no saved identity"; the real client-side store takes over once WebAssembly boots.
    /// </summary>
    public sealed class NullIdentityKeyStore : IIdentityKeyStore
    {
        public Task SaveAsync(ChayIdentity identity) => Task.CompletedTask;

        public Task<ChayIdentity?> LoadAsync() => Task.FromResult<ChayIdentity?>(null);

        public Task ClearAsync() => Task.CompletedTask;
    }
}
