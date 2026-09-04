using ChaySocialSonnet.MainProject.Backend;
using System.Net.Http.Json;

namespace ChaySocialSonnet.MainProject.Services.Identity
{
    /// <summary>
    /// Talks to the server's /api/identity/* endpoints so a locally generated <see cref="ChayIdentity"/>
    /// can register its public keys and later prove ownership via challenge-response — the private keys
    /// never leave this call site; only public keys, a challenge nonce, and a signature cross the wire.
    /// </summary>
    public sealed class IdentityApiClient(HttpClient httpClient)
    {
        /// <summary> Registers <paramref name="identity"/>'s public keys with the server under its <see cref="ChayIdentity.PublicId"/>. </summary>
        public async Task RegisterAsync(ChayIdentity identity, string displayName)
        {
            var request = new RegisterIdentityRequest(
                identity.PublicId,
                Convert.ToBase64String(identity.SigningPublicKey),
                Convert.ToBase64String(identity.EncryptionPublicKey),
                displayName);

            HttpResponseMessage response = await httpClient.PostAsJsonAsync("/api/identity/register", request);
            response.EnsureSuccessStatusCode();
        }

        /// <summary> Runs the full challenge-response round trip: requests a nonce, signs it locally with the private key, and asks the server to verify the signature. Returns whether the server accepted it. </summary>
        public async Task<bool> SignInAsync(ChayIdentity identity)
        {
            HttpResponseMessage challengeHttpResponse = await httpClient.PostAsJsonAsync("/api/identity/challenge", new IssueChallengeRequest(identity.PublicId));
            challengeHttpResponse.EnsureSuccessStatusCode();
            IssueChallengeResponse challengeResponse = (await challengeHttpResponse.Content.ReadFromJsonAsync<IssueChallengeResponse>())!;

            byte[] signature = IdentityService.Sign(identity.SigningPrivateKey, Convert.FromBase64String(challengeResponse.Challenge));

            HttpResponseMessage verifyHttpResponse = await httpClient.PostAsJsonAsync(
                "/api/identity/verify",
                new VerifyChallengeRequest(identity.PublicId, challengeResponse.Challenge, Convert.ToBase64String(signature)));
            verifyHttpResponse.EnsureSuccessStatusCode();
            VerifyChallengeResponse verifyResponse = (await verifyHttpResponse.Content.ReadFromJsonAsync<VerifyChallengeResponse>())!;

            return verifyResponse.Success;
        }

        /// <summary> Looks up the display name for a public id, or null if nobody registered under it. </summary>
        public async Task<IdentitySummary?> GetSummaryAsync(string publicId)
        {
            HttpResponseMessage response = await httpClient.GetAsync($"/api/identity/{Uri.EscapeDataString(publicId)}");
            return response.StatusCode == System.Net.HttpStatusCode.NotFound
                ? null
                : await response.Content.ReadFromJsonAsync<IdentitySummary>();
        }

        /// <summary> Finds registered identities whose public id or display name starts with <paramref name="query"/>. </summary>
        public async Task<IReadOnlyList<IdentitySummary>> SearchAsync(string query, int count)
        {
            List<IdentitySummary>? results = await httpClient.GetFromJsonAsync<List<IdentitySummary>>(
                $"/api/identity/search?query={Uri.EscapeDataString(query)}&count={count}");
            return results ?? [];
        }

        /// <summary> Looks up a registered identity's ML-KEM encryption public key, so a message can be encrypted to it. Null if unregistered. </summary>
        public async Task<byte[]?> GetEncryptionPublicKeyAsync(string publicId)
        {
            HttpResponseMessage response = await httpClient.GetAsync($"/api/identity/{Uri.EscapeDataString(publicId)}/encryption-key");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            string base64 = (await response.Content.ReadFromJsonAsync<string>())!;
            return Convert.FromBase64String(base64);
        }
    }
}
