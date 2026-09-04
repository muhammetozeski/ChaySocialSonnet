namespace ChaySocialSonnet.MainProject.Services.Identity
{
    /// <summary>
    /// Persists the signed-in <see cref="ChayIdentity"/> on THIS device only — never over the network.
    /// Each host provides its own implementation matching its platform's secure storage: MAUI uses
    /// <c>Microsoft.Maui.Storage.SecureStorage</c>, the Blazor Web App's WebAssembly client uses the
    /// browser's localStorage, and the server's static prerender pass uses <see cref="NullIdentityKeyStore"/>
    /// since it must never touch device-local storage at all.
    /// </summary>
    public interface IIdentityKeyStore
    {
        /// <summary> Persists <paramref name="identity"/>, overwriting any previously saved identity. </summary>
        Task SaveAsync(ChayIdentity identity);

        /// <summary> Loads the previously saved identity, or null if none was ever saved (or it was cleared). </summary>
        Task<ChayIdentity?> LoadAsync();

        /// <summary> Removes any saved identity. </summary>
        Task ClearAsync();
    }
}
