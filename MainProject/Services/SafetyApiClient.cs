using ChaySocialSonnet.MainProject.Backend;
using System.Net.Http.Json;

namespace ChaySocialSonnet.MainProject.Services
{
    /// <summary> Talks to the server's /api/block/* and /api/report endpoints. </summary>
    public sealed class SafetyApiClient(HttpClient httpClient)
    {
        public async Task BlockAsync(string blockedPublicId)
        {
            HttpRequestMessage request = AuthorizedRequests.Create(HttpMethod.Post, $"/api/block/{Uri.EscapeDataString(blockedPublicId)}");
            HttpResponseMessage response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        public async Task UnblockAsync(string blockedPublicId)
        {
            HttpRequestMessage request = AuthorizedRequests.Create(HttpMethod.Delete, $"/api/block/{Uri.EscapeDataString(blockedPublicId)}");
            HttpResponseMessage response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        public async Task<bool> IsBlockedAsync(string blockerPublicId, string blockedPublicId) =>
            await httpClient.GetFromJsonAsync<bool>($"/api/block/{Uri.EscapeDataString(blockedPublicId)}/status?blockerPublicId={Uri.EscapeDataString(blockerPublicId)}");

        public async Task SubmitReportAsync(string targetType, string targetId, string reason)
        {
            HttpRequestMessage request = AuthorizedRequests.Create(HttpMethod.Post, "/api/report", new SubmitReportRequest(targetType, targetId, reason));
            HttpResponseMessage response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }
    }
}
