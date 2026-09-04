using ChaySocialSonnet.MainProject.Backend;
using System.Net.Http.Json;

namespace ChaySocialSonnet.MainProject.Services
{
    /// <summary> Talks to the server's /api/block/* and /api/report endpoints. </summary>
    public sealed class SafetyApiClient(HttpClient httpClient)
    {
        public async Task BlockAsync(string blockerPublicId, string blockedPublicId)
        {
            HttpResponseMessage response = await httpClient.PostAsJsonAsync($"/api/block/{Uri.EscapeDataString(blockedPublicId)}", new BlockRequest(blockerPublicId));
            response.EnsureSuccessStatusCode();
        }

        public async Task UnblockAsync(string blockerPublicId, string blockedPublicId)
        {
            HttpRequestMessage request = new(HttpMethod.Delete, $"/api/block/{Uri.EscapeDataString(blockedPublicId)}")
            {
                Content = JsonContent.Create(new BlockRequest(blockerPublicId))
            };
            HttpResponseMessage response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        public async Task<bool> IsBlockedAsync(string blockerPublicId, string blockedPublicId) =>
            await httpClient.GetFromJsonAsync<bool>($"/api/block/{Uri.EscapeDataString(blockedPublicId)}/status?blockerPublicId={Uri.EscapeDataString(blockerPublicId)}");

        public async Task SubmitReportAsync(string reporterPublicId, string targetType, string targetId, string reason)
        {
            HttpResponseMessage response = await httpClient.PostAsJsonAsync("/api/report", new SubmitReportRequest(reporterPublicId, targetType, targetId, reason));
            response.EnsureSuccessStatusCode();
        }
    }
}
