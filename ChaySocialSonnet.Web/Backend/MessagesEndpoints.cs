using ChaySocialSonnet.MainProject.Backend;
using Microsoft.AspNetCore.Mvc;

namespace ChaySocialSonnet.Web.Backend
{
    /// <summary> Maps the /api/messages/* endpoints backing <see cref="MainProject.Services.MessagesApiClient"/>. The server only ever sees an ML-KEM encapsulated key and an AES ciphertext — never a private key or the plaintext message. </summary>
    public static class MessagesEndpoints
    {
        public static void MapMessagesEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/messages/send", async (SendMessageRequest request, IMessageRelay relay, [FromHeader(Name = "Authorization")] string? authorization, IIdentityRegistry registry) =>
            {
                string? actingPublicId = await RequestAuthentication.ResolveActingPublicIdAsync(authorization, registry);
                if (actingPublicId is null)
                {
                    return Results.Unauthorized();
                }

                byte[] encapsulatedKey, ciphertext;
                try
                {
                    encapsulatedKey = Convert.FromBase64String(request.EncapsulatedKeyBase64);
                    ciphertext = Convert.FromBase64String(request.CiphertextBase64);
                }
                catch (FormatException)
                {
                    return Results.BadRequest("encapsulatedKeyBase64/ciphertextBase64 must be valid Base64.");
                }

                await relay.SendAsync(actingPublicId, request.RecipientPublicId, encapsulatedKey, ciphertext);
                return Results.Ok();
            });

            app.MapGet("/api/messages/inbox/{publicId}", async (string publicId, IMessageRelay relay, [FromHeader(Name = "Authorization")] string? authorization, IIdentityRegistry registry) =>
            {
                string? actingPublicId = await RequestAuthentication.ResolveActingPublicIdAsync(authorization, registry);
                if (actingPublicId is null || actingPublicId != publicId)
                {
                    return Results.Unauthorized();
                }

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
