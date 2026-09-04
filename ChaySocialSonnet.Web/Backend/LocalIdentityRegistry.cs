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

        public Task RegisterAsync(string publicId, byte[] signingPublicKey, byte[] encryptionPublicKey, string displayName)
        {
            identities[publicId] = new RegisteredIdentity(signingPublicKey, encryptionPublicKey, displayName);
            return Task.CompletedTask;
        }

        public Task<byte[]?> GetSigningPublicKeyAsync(string publicId) =>
            Task.FromResult(identities.TryGetValue(publicId, out RegisteredIdentity? identity) ? identity.SigningPublicKey : null);

        public Task<byte[]?> GetEncryptionPublicKeyAsync(string publicId) =>
            Task.FromResult(identities.TryGetValue(publicId, out RegisteredIdentity? identity) ? identity.EncryptionPublicKey : null);

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
