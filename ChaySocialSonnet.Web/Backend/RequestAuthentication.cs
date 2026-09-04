using ChaySocialSonnet.MainProject.Backend;

namespace ChaySocialSonnet.Web.Backend
{
    /// <summary>
    /// Resolves the identity actually making a request from its "Authorization: Bearer &lt;session token&gt;"
    /// header (issued by <see cref="IIdentityRegistry.VerifyChallengeAsync"/>). Every mutating endpoint must
    /// derive the acting public id through this — never trust a public id field the client puts in its own
    /// request body, since that would let anyone act as anyone else just by naming their id.
    /// </summary>
    public static class RequestAuthentication
    {
        const string BearerPrefix = "Bearer ";

        /// <summary> Null if there's no bearer token, or it doesn't resolve to a live session. </summary>
        public static async Task<string?> ResolveActingPublicIdAsync(string? authorizationHeader, IIdentityRegistry registry)
        {
            if (authorizationHeader is null || !authorizationHeader.StartsWith(BearerPrefix, StringComparison.Ordinal))
            {
                return null;
            }

            string token = authorizationHeader[BearerPrefix.Length..];
            return await registry.ResolveSessionAsync(token);
        }
    }
}
