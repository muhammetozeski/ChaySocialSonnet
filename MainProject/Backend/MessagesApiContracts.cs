namespace ChaySocialSonnet.MainProject.Backend
{
    /// <summary>
    /// Shared request/response shapes for the server's /api/messages/* endpoints, referenced by both the
    /// endpoint handlers (ChaySocialSonnet.Web/Backend/MessagesEndpoints.cs) and the client
    /// (MainProject/Services/MessagesApiClient.cs) so the two can never drift apart. The sender on
    /// <see cref="SendMessageRequest"/> is always the caller resolved server-side from their session
    /// token, never a field in the request — a client can't send a message impersonating anyone else.
    /// </summary>
    public sealed record SendMessageRequest(string RecipientPublicId, string EncapsulatedKeyBase64, string CiphertextBase64);

    public sealed record EncryptedMessageResponse(string SenderPublicId, string EncapsulatedKeyBase64, string CiphertextBase64, DateTimeOffset SentAt);
}
