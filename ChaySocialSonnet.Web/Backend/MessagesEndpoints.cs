using ChaySocialSonnet.MainProject.Backend;

namespace ChaySocialSonnet.Web.Backend
{
    /// <summary> Maps the /api/messages/* endpoints backing <see cref="MainProject.Services.MessagesApiClient"/>. The server only ever sees an ML-KEM encapsulated key and an AES ciphertext — never a private key or the plaintext message. </summary>
    public static class MessagesEndpoints
    {
        public static void MapMessagesEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/messages/send", async (SendMessageRequest request, IMessageRelay relay) =>
            {
                await relay.SendAsync(
                    request.SenderPublicId,
                    request.RecipientPublicId,
                    Convert.FromBase64String(request.EncapsulatedKeyBase64),
                    Convert.FromBase64String(request.CiphertextBase64));
                return Results.Ok();
            });

            app.MapGet("/api/messages/inbox/{publicId}", async (string publicId, IMessageRelay relay) =>
            {
                IReadOnlyList<EncryptedMessage> messages = await relay.GetInboxAsync(publicId);
                IEnumerable<EncryptedMessageResponse> response = messages.Select(message => new EncryptedMessageResponse(
                    message.SenderPublicId,
                    Convert.ToBase64String(message.EncapsulatedKey),
                    Convert.ToBase64String(message.Ciphertext),
                    message.SentAt));
                return Results.Ok(response);
            });
        }
    }
}
