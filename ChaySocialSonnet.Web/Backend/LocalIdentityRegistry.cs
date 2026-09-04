using ChaySocialSonnet.MainProject.Backend;
using ChaySocialSonnet.MainProject.Services.Identity;
using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace ChaySocialSonnet.Web.Backend
{
    /// <summary>
    /// In-memory <see cref="IIdentityRegistry"/> used while the project runs against the developer's
    /// own PC as its server. Registrations and pending challenges are lost on restart — swap in a
    /// persistent implementation before this matters for real users.
    /// </summary>
    public sealed class LocalIdentityRegistry : IIdentityRegistry
    {
        const int ChallengeNonceLengthBytes = 32;
        static readonly TimeSpan ChallengeLifetime = TimeSpan.FromMinutes(5);

        sealed record RegisteredIdentity(byte[] SigningPublicKey, byte[] EncryptionPublicKey, string DisplayName);
        sealed record PendingChallenge(string Nonce, DateTimeOffset ExpiresAt);

        readonly ConcurrentDictionary<string, RegisteredIdentity> identities = new();
        readonly ConcurrentDictionary<string, PendingChallenge> pendingChallenges = new();

        public Task<RegisterIdentityResult> RegisterAsync(string publicId, byte[] signingPublicKey, byte[] encryptionPublicKey, string displayName)
        {
            if (IdentityService.DerivePublicId(signingPublicKey) != publicId)
            {
                return Task.FromResult(RegisterIdentityResult.PublicIdMismatch);
            }

            // Unreachable without a SHA-256 collision: the mismatch check above already guarantees
            // signingPublicKey hashes to publicId, so a genuinely different key can never reach here under
            // the same publicId. Kept as defense-in-depth in case a future change loosens that guarantee.
            if (identities.TryGetValue(publicId, out RegisteredIdentity? existing) && !existing.SigningPublicKey.SequenceEqual(signingPublicKey))
            {
                return Task.FromResult(RegisterIdentityResult.AlreadyRegisteredWithDifferentKey);
            }

            identities[publicId] = new RegisteredIdentity(signingPublicKey, encryptionPublicKey, displayName);
            return Task.FromResult(RegisterIdentityResult.Registered);
        }

        public Task<byte[]?> GetSigningPublicKeyAsync(string publicId) =>
            Task.FromResult(identities.TryGetValue(publicId, out RegisteredIdentity? identity) ? identity.SigningPublicKey : null);

        public Task<byte[]?> GetEncryptionPublicKeyAsync(string publicId) =>
            Task.FromResult(identities.TryGetValue(publicId, out RegisteredIdentity? identity) ? identity.EncryptionPublicKey : null);

        public Task<IdentitySummary?> GetSummaryAsync(string publicId)
        {
            IdentitySummary? summary = identities.TryGetValue(publicId, out RegisteredIdentity? identity)
                ? new IdentitySummary(publicId, identity.DisplayName)
                : null;
            return Task.FromResult(summary);
        }

        public Task<IReadOnlyList<IdentitySummary>> SearchAsync(string query, int count)
        {
            IReadOnlyList<IdentitySummary> results = identities
                .Where(pair => pair.Key.StartsWith(query, StringComparison.OrdinalIgnoreCase)
                    || pair.Value.DisplayName.StartsWith(query, StringComparison.OrdinalIgnoreCase))
                .Take(count)
                .Select(pair => new IdentitySummary(pair.Key, pair.Value.DisplayName))
                .ToList();
            return Task.FromResult(results);
        }

        public Task<string> IssueChallengeAsync(string publicId)
        {
            string nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(ChallengeNonceLengthBytes));
            pendingChallenges[publicId] = new PendingChallenge(nonce, DateTimeOffset.UtcNow.Add(ChallengeLifetime));
            return Task.FromResult(nonce);
        }

        public Task<bool> VerifyChallengeAsync(string publicId, string challenge, byte[] signature)
        {
            bool challengeStillValid = pendingChallenges.TryRemove(publicId, out PendingChallenge? pending)
                && pending.ExpiresAt >= DateTimeOffset.UtcNow
                && pending.Nonce == challenge;

            if (!challengeStillValid || !identities.TryGetValue(publicId, out RegisteredIdentity? identity))
            {
                return Task.FromResult(false);
            }

            byte[] challengeBytes = Convert.FromBase64String(challenge);
            return Task.FromResult(IdentityService.Verify(identity.SigningPublicKey, challengeBytes, signature));
        }
    }
}
