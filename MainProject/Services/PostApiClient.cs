using ChaySocialSonnet.MainProject.Backend;
using System.Net.Http.Json;

namespace ChaySocialSonnet.MainProject.Services
{
    /// <summary> Talks to the server's /api/posts/* endpoints. Public posts are not encrypted (see PublicPost's own summary), so this is a plain HTTP read, unlike the identity/message flows. </summary>
    public sealed class PostApiClient(HttpClient httpClient)
    {
        /// <summary> Fetches the most recent public posts, newest first. </summary>
        public async Task<IReadOnlyList<PublicPost>> GetRecentPostsAsync(int count)
        {
            List<PublicPost>? posts = await httpClient.GetFromJsonAsync<List<PublicPost>>($"/api/posts/recent?count={count}");
            return posts ?? [];
        }
    }
}
