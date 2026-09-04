using ChaySocialSonnet.MainProject.Backend;
using System.Net.Http.Json;

namespace ChaySocialSonnet.MainProject.Services
{
    /// <summary> Talks to the server's /api/messages/* endpoints. The server relays an ML-KEM encapsulated key and an AES ciphertext only — it never sees a private key or the decrypted message content. </summary>
    public sealed class MessagesApiClient(HttpClient httpClient)
    {
        /// <summary> Sends an already-encrypted message to <paramref name="recipientPublicId"/>. </summary>
        public async Task SendAsync(string senderPublicId, string recipientPublicId, byte[] encapsulatedKey, byte[] ciphertext)
        {
            var request = new SendMessageRequest(
                senderPublicId,
                recipientPublicId,
                Convert.ToBase64String(encapsulatedKey),
                Convert.ToBase64String(ciphertext));

            HttpResponseMessage response = await httpClient.PostAsJsonAsync("/api/messages/send", request);
            response.EnsureSuccessStatusCode();
        }

        /// <summary> Fetches every encrypted message waiting for <paramref name="publicId"/>, oldest first. </summary>
        public async Task<IReadOnlyList<EncryptedMessageResponse>> GetInboxAsync(string publicId)
        {
            List<EncryptedMessageResponse>? messages = await httpClient.GetFromJsonAsync<List<EncryptedMessageResponse>>(
                $"/api/messages/inbox/{Uri.EscapeDataString(publicId)}");
            return messages ?? [];
        }
    }
}
