using ChaySocialSonnet.MainProject.Backend;
using System.Net.Http.Json;

namespace ChaySocialSonnet.MainProject.Services
{
    /// <summary> Talks to the server's /api/notifications/* endpoints. Every call requires the caller to be signed in as <paramref name="publicId"/> itself — the server checks the session token. </summary>
    public sealed class NotificationsApiClient(HttpClient httpClient)
    {
        public async Task<IReadOnlyList<NotificationResponse>> GetForUserAsync(string publicId, int count)
        {
            HttpRequestMessage request = AuthorizedRequests.Create(HttpMethod.Get, $"/api/notifications/{Uri.EscapeDataString(publicId)}?count={count}");
            HttpResponseMessage response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            List<NotificationResponse>? notifications = await response.Content.ReadFromJsonAsync<List<NotificationResponse>>();
            return notifications ?? [];
        }

        public async Task<int> GetUnreadCountAsync(string publicId)
        {
            HttpRequestMessage request = AuthorizedRequests.Create(HttpMethod.Get, $"/api/notifications/{Uri.EscapeDataString(publicId)}/unread-count");
            HttpResponseMessage response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<int>();
        }

        public async Task MarkAllReadAsync(string publicId)
        {
            HttpRequestMessage request = AuthorizedRequests.Create(HttpMethod.Post, $"/api/notifications/{Uri.EscapeDataString(publicId)}/mark-read");
            HttpResponseMessage response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }
    }
}
