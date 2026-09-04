using ChaySocialSonnet.MainProject.Backend;

namespace ChaySocialSonnet.Web.Backend
{
    /// <summary> Maps the /api/identity/* endpoints backing <see cref="IdentityApiClient"/>: register a generated identity's public keys, then prove ownership of it via challenge-response — the private key is never part of any request. </summary>
    public static class IdentityEndpoints
    {
        public static void MapIdentityEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/identity/register", async (RegisterIdentityRequest request, IIdentityRegistry registry) =>
            {
                if (!TryDecodeBase64(request.SigningPublicKeyBase64, out byte[] signingPublicKey))
                {
                    return Results.BadRequest("signingPublicKeyBase64 is not valid Base64.");
                }
                if (!TryDecodeBase64(request.EncryptionPublicKeyBase64, out byte[] encryptionPublicKey))
                {
                    return Results.BadRequest("encryptionPublicKeyBase64 is not valid Base64.");
                }

                RegisterIdentityResult result = await registry.RegisterAsync(request.PublicId, signingPublicKey, encryptionPublicKey, request.DisplayName);
                return result switch
                {
                    RegisterIdentityResult.Registered => Results.Ok(),
                    RegisterIdentityResult.PublicIdMismatch => Results.BadRequest("publicId does not match the given signing public key."),
                    RegisterIdentityResult.AlreadyRegisteredWithDifferentKey => Results.Conflict("This identity is already registered with a different key."),
                    _ => Results.Problem("Unknown registration result.")
                };
            });

            app.MapPost("/api/identity/challenge", async (IssueChallengeRequest request, IIdentityRegistry registry) =>
            {
                string challenge = await registry.IssueChallengeAsync(request.PublicId);
                return Results.Ok(new IssueChallengeResponse(challenge));
            });

            app.MapPost("/api/identity/verify", async (VerifyChallengeRequest request, IIdentityRegistry registry) =>
            {
                if (!TryDecodeBase64(request.SignatureBase64, out byte[] signature))
                {
                    return Results.BadRequest("signatureBase64 is not valid Base64.");
                }

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

        /// <summary> These three endpoints are pre-authentication — a malformed key/signature must come back as a clean 400, not an unhandled <see cref="FormatException"/> that takes the request down. </summary>
        static bool TryDecodeBase64(string base64, out byte[] decoded)
        {
            try
            {
                decoded = Convert.FromBase64String(base64);
                return true;
            }
            catch (FormatException)
            {
                decoded = [];
                return false;
            }
        }
    }
}
