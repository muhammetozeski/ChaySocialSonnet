using ChaySocialSonnet.MainProject.Backend;
using System.Net.Http.Json;

namespace ChaySocialSonnet.MainProject.Services
{
    /// <summary> Talks to the server's /api/messages/* endpoints. The server only ever sees an ML-KEM encapsulated key and an AES ciphertext — it never sees a private key or the decrypted message content. </summary>
    public sealed class MessagesApiClient(HttpClient httpClient)
    {
        /// <summary> Sends an already-encrypted message to <paramref name="recipientPublicId"/>, as the current session's identity (see <see cref="AuthService.SessionToken"/>) — there is no "send as someone else" option. </summary>
        public async Task SendAsync(string recipientPublicId, byte[] encapsulatedKey, byte[] ciphertext)
        {
            var body = new SendMessageRequest(recipientPublicId, Convert.ToBase64String(encapsulatedKey), Convert.ToBase64String(ciphertext));
            HttpRequestMessage request = AuthorizedRequests.Create(HttpMethod.Post, "/api/messages/send", body);
            HttpResponseMessage response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        /// <summary> Fetches every encrypted message waiting for <paramref name="publicId"/>, oldest first. Only the signed-in owner of that inbox can read it — the server checks the session token matches. </summary>
        public async Task<IReadOnlyList<EncryptedMessageResponse>> GetInboxAsync(string publicId)
        {
            HttpRequestMessage request = AuthorizedRequests.Create(HttpMethod.Get, $"/api/messages/inbox/{Uri.EscapeDataString(publicId)}");
            HttpResponseMessage response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            List<EncryptedMessageResponse>? messages = await response.Content.ReadFromJsonAsync<List<EncryptedMessageResponse>>();
            return messages ?? [];
        }
    }
}
