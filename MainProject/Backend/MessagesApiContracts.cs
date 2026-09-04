namespace ChaySocialSonnet.MainProject.Backend
{
    /// <summary> Shared request/response shapes for the server's /api/messages/* endpoints, referenced by both the endpoint handlers (ChaySocialSonnet.Web/Backend/MessagesEndpoints.cs) and the client (MainProject/Services/MessagesApiClient.cs) so the two can never drift apart. </summary>
    public sealed record SendMessageRequest(string SenderPublicId, string RecipientPublicId, string EncapsulatedKeyBase64, string CiphertextBase64);

    public sealed record EncryptedMessageResponse(string SenderPublicId, string EncapsulatedKeyBase64, string CiphertextBase64, DateTimeOffset SentAt);
}
