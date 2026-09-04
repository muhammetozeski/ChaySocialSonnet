using ChaySocialSonnet.MainProject.Backend;
using System.Net.Http.Json;

namespace ChaySocialSonnet.MainProject.Services
{
    /// <summary> Talks to the server's /api/follow/* endpoints. </summary>
    public sealed class FollowApiClient(HttpClient httpClient)
    {
        public async Task<FollowStatusResponse> FollowAsync(string targetPublicId)
        {
            HttpRequestMessage request = AuthorizedRequests.Create(HttpMethod.Post, $"/api/follow/{Uri.EscapeDataString(targetPublicId)}");
            HttpResponseMessage response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<FollowStatusResponse>())!;
        }

        public async Task<FollowStatusResponse> UnfollowAsync(string targetPublicId)
        {
            HttpRequestMessage request = AuthorizedRequests.Create(HttpMethod.Delete, $"/api/follow/{Uri.EscapeDataString(targetPublicId)}");
            HttpResponseMessage response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<FollowStatusResponse>())!;
        }

        public async Task<FollowStatusResponse> GetStatusAsync(string targetPublicId, string? viewerPublicId)
        {
            string viewerQuery = viewerPublicId is null ? "" : $"?viewerPublicId={Uri.EscapeDataString(viewerPublicId)}";
            return (await httpClient.GetFromJsonAsync<FollowStatusResponse>($"/api/follow/{Uri.EscapeDataString(targetPublicId)}/status{viewerQuery}"))!;
        }

        /// <summary> Public ids of everyone <paramref name="publicId"/> follows, for building a following-only feed. </summary>
        public async Task<IReadOnlyList<string>> GetFollowingIdsAsync(string publicId)
        {
            List<string>? ids = await httpClient.GetFromJsonAsync<List<string>>($"/api/follow/{Uri.EscapeDataString(publicId)}/following-ids");
            return ids ?? [];
        }
    }
}
