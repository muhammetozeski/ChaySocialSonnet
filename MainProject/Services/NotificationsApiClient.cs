using ChaySocialSonnet.MainProject.Backend;
using System.Net.Http.Json;

namespace ChaySocialSonnet.MainProject.Services
{
    /// <summary> Talks to the server's /api/notifications/* endpoints. </summary>
    public sealed class NotificationsApiClient(HttpClient httpClient)
    {
        public async Task<IReadOnlyList<NotificationResponse>> GetForUserAsync(string publicId, int count)
        {
            List<NotificationResponse>? notifications = await httpClient.GetFromJsonAsync<List<NotificationResponse>>($"/api/notifications/{Uri.EscapeDataString(publicId)}?count={count}");
            return notifications ?? [];
        }

        public async Task<int> GetUnreadCountAsync(string publicId) =>
            await httpClient.GetFromJsonAsync<int>($"/api/notifications/{Uri.EscapeDataString(publicId)}/unread-count");

        public async Task MarkAllReadAsync(string publicId)
        {
            HttpResponseMessage response = await httpClient.PostAsync($"/api/notifications/{Uri.EscapeDataString(publicId)}/mark-read", content: null);
            response.EnsureSuccessStatusCode();
        }
    }
}
