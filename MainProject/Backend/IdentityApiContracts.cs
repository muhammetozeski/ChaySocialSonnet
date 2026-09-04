namespace ChaySocialSonnet.MainProject.Backend
{
    /// <summary> Shared request/response shapes for the server's /api/identity/* endpoints, referenced by both the endpoint handlers (ChaySocialSonnet.Web/Program.cs) and the client (MainProject/Services/Identity/IdentityApiClient.cs) so the two can never drift apart. </summary>
    public sealed record RegisterIdentityRequest(string PublicId, string SigningPublicKeyBase64, string EncryptionPublicKeyBase64, string DisplayName);

    public sealed record IssueChallengeRequest(string PublicId);

    public sealed record IssueChallengeResponse(string Challenge);

    public sealed record VerifyChallengeRequest(string PublicId, string Challenge, string SignatureBase64);

    /// <summary> <see cref="SessionToken"/> is null when <see cref="Success"/> is false; otherwise it proves control of the identity for subsequent mutating requests (send it as an "Authorization: Bearer" header). </summary>
    public sealed record VerifyChallengeResponse(bool Success, string? SessionToken);
}
