using ChaySocialSonnet.MainProject.Backend;
using ChaySocialSonnet.MainProject.Services.Identity;

namespace ChaySocialSonnet.Web.Backend
{
    /// <summary> Maps the /api/identity/* endpoints backing <see cref="IdentityApiClient"/>: register a generated identity's public keys, then prove ownership of it via challenge-response — the private key is never part of any request. </summary>
    public static class IdentityEndpoints
    {
        public static void MapIdentityEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/identity/register", async (RegisterIdentityRequest request, IIdentityRegistry registry) =>
            {
                byte[] signingPublicKey = Convert.FromBase64String(request.SigningPublicKeyBase64);
                byte[] encryptionPublicKey = Convert.FromBase64String(request.EncryptionPublicKeyBase64);

                if (IdentityService.DerivePublicId(signingPublicKey) != request.PublicId)
                {
                    return Results.BadRequest("publicId does not match the given signing public key.");
                }

                byte[]? existingSigningPublicKey = await registry.GetSigningPublicKeyAsync(request.PublicId);
                if (existingSigningPublicKey is not null && !existingSigningPublicKey.SequenceEqual(signingPublicKey))
                {
                    return Results.Conflict("This identity is already registered with a different key.");
                }

                await registry.RegisterAsync(request.PublicId, signingPublicKey, encryptionPublicKey, request.DisplayName);
                return Results.Ok();
            });

            app.MapPost("/api/identity/challenge", async (IssueChallengeRequest request, IIdentityRegistry registry) =>
            {
                string challenge = await registry.IssueChallengeAsync(request.PublicId);
                return Results.Ok(new IssueChallengeResponse(challenge));
            });

            app.MapPost("/api/identity/verify", async (VerifyChallengeRequest request, IIdentityRegistry registry) =>
            {
                byte[] signature = Convert.FromBase64String(request.SignatureBase64);
                bool success = await registry.VerifyChallengeAsync(request.PublicId, request.Challenge, signature);
                return Results.Ok(new VerifyChallengeResponse(success));
            });

            app.MapGet("/api/identity/{publicId}", async (string publicId, IIdentityRegistry registry) =>
            {
                IdentitySummary? summary = await registry.GetSummaryAsync(publicId);
                return summary is null ? Results.NotFound() : Results.Ok(summary);
            });

            app.MapGet("/api/identity/search", async (string query, int count, IIdentityRegistry registry) =>
                Results.Ok(await registry.SearchAsync(query, count)));

            app.MapGet("/api/identity/{publicId}/encryption-key", async (string publicId, IIdentityRegistry registry) =>
            {
                byte[]? key = await registry.GetEncryptionPublicKeyAsync(publicId);
                return key is null ? Results.NotFound() : Results.Ok(Convert.ToBase64String(key));
            });
        }
    }
}
