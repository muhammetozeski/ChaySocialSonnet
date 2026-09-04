using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ChaySocialSonnet.MainProject.Services
{
    /// <summary>
    /// Builds an <see cref="HttpRequestMessage"/> carrying the current session's bearer token
    /// (<see cref="AuthService.SessionToken"/>), for the mutating API-client calls that need the server to
    /// know who is really calling rather than trusting a client-supplied public id. If there's no session
    /// (signed out), the request goes out without an Authorization header and the server rejects it with 401
    /// — the same as any other unauthenticated call.
    /// </summary>
    static class AuthorizedRequests
    {
        public static HttpRequestMessage Create(HttpMethod method, string url, object? jsonBody = null)
        {
            var request = new HttpRequestMessage(method, url);
            if (jsonBody is not null)
            {
                request.Content = JsonContent.Create(jsonBody);
            }
            if (AuthService.SessionToken is string token)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            return request;
        }
    }
}
