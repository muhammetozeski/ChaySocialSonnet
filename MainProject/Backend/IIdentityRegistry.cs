namespace ChaySocialSonnet.MainProject.Backend
{
    /// <summary>
    /// Server-side identity directory: maps a public id to its ML-DSA/ML-KEM public keys and runs the
    /// challenge-response sign-in flow. The private key never appears in any of these calls — only the
    /// public id, public keys, a challenge nonce, and a signature cross this boundary. Implemented
    /// locally for now (<c>ChaySocialSonnet.Web</c>'s in-memory registry); swapping in a Firebase-backed
    /// implementation later means registering a different <see cref="IIdentityRegistry"/> in DI, nothing else.
    /// </summary>
    /// <summary> Minimal public info about a registered identity, used for search results and viewing another user's profile. </summary>
    public sealed record IdentitySummary(string PublicId, string DisplayName);

    /// <summary> Outcome of <see cref="IIdentityRegistry.RegisterAsync"/> — the two account-takeover checks every implementation must enforce. </summary>
    public enum RegisterIdentityResult
    {
        Registered,

        /// <summary> <c>publicId</c> is not the hash of the given signing public key. </summary>
        PublicIdMismatch,

        /// <summary> <c>publicId</c> is already registered under a different signing public key. </summary>
        AlreadyRegisteredWithDifferentKey
    }

    public interface IIdentityRegistry
    {
        /// <summary>
        /// Registers a freshly generated identity's public keys and chosen display name. Every
        /// implementation must reject a <c>publicId</c> that doesn't match the signing public key's hash
        /// and must reject re-registering an existing <c>publicId</c> under a different signing key — these
        /// are the system's only account-takeover protections, so they live here rather than in the endpoint.
        /// </summary>
        Task<RegisterIdentityResult> RegisterAsync(string publicId, byte[] signingPublicKey, byte[] encryptionPublicKey, string displayName);

        /// <summary> Looks up the display name and public id for a registered identity, or null if unregistered. </summary>
        Task<IdentitySummary?> GetSummaryAsync(string publicId);

        /// <summary> Finds registered identities whose public id or display name starts with <paramref name="query"/>. </summary>
        Task<IReadOnlyList<IdentitySummary>> SearchAsync(string query, int count);

        /// <summary> Looks up the ML-DSA signing public key for a registered identity, or null if unregistered. </summary>
        Task<byte[]?> GetSigningPublicKeyAsync(string publicId);

        /// <summary> Looks up the ML-KEM encryption public key for a registered identity, or null if unregistered. </summary>
        Task<byte[]?> GetEncryptionPublicKeyAsync(string publicId);

        /// <summary> Issues a fresh, single-use challenge nonce for <paramref name="publicId"/> to sign as proof of holding the matching private key. </summary>
        Task<string> IssueChallengeAsync(string publicId);

        /// <summary> Verifies a signature over a previously issued challenge. The challenge is consumed (single-use) whether verification succeeds or fails, so it can never be replayed. </summary>
        Task<bool> VerifyChallengeAsync(string publicId, string challenge, byte[] signature);
    }
}
