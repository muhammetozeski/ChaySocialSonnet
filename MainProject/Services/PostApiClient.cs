using ChaySocialSonnet.MainProject.Backend;
using System.Net.Http.Json;

namespace ChaySocialSonnet.MainProject.Services
{
    /// <summary> Talks to the server's /api/posts/* endpoints. Public posts are not encrypted (see PublicPost's own summary), so this is a plain HTTP read/write, unlike the identity/message flows. </summary>
    public sealed class PostApiClient(HttpClient httpClient)
    {
        /// <summary> Fetches the most recent public posts, newest first, with like/comment stats already resolved for <paramref name="viewerPublicId"/>. </summary>
        public async Task<IReadOnlyList<PostSummary>> GetRecentPostsAsync(int count, string? viewerPublicId)
        {
            string viewerQuery = viewerPublicId is null ? "" : $"&viewerPublicId={Uri.EscapeDataString(viewerPublicId)}";
            List<PostSummary>? posts = await httpClient.GetFromJsonAsync<List<PostSummary>>($"/api/posts/recent?count={count}{viewerQuery}");
            return posts ?? [];
        }

        /// <summary> Fetches the most recent posts by one author, newest first. </summary>
        public async Task<IReadOnlyList<PostSummary>> GetPostsByAuthorAsync(string authorPublicId, int count, string? viewerPublicId)
        {
            string viewerQuery = viewerPublicId is null ? "" : $"&viewerPublicId={Uri.EscapeDataString(viewerPublicId)}";
            List<PostSummary>? posts = await httpClient.GetFromJsonAsync<List<PostSummary>>($"/api/posts/by-author/{Uri.EscapeDataString(authorPublicId)}?count={count}{viewerQuery}");
            return posts ?? [];
        }

        public async Task<PostSummary> CreatePostAsync(string authorPublicId, string text)
        {
            HttpResponseMessage response = await httpClient.PostAsJsonAsync("/api/posts", new CreatePostRequest(authorPublicId, text));
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<PostSummary>())!;
        }

        public async Task DeletePostAsync(string postId, string requestingPublicId)
        {
            HttpRequestMessage request = new(HttpMethod.Delete, $"/api/posts/{Uri.EscapeDataString(postId)}")
            {
                Content = JsonContent.Create(new DeletePostRequest(requestingPublicId))
            };
            HttpResponseMessage response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        public async Task<ToggleLikeResponse> ToggleLikeAsync(string postId, string likerPublicId)
        {
            HttpResponseMessage response = await httpClient.PostAsJsonAsync($"/api/posts/{Uri.EscapeDataString(postId)}/like", new ToggleLikeRequest(likerPublicId));
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<ToggleLikeResponse>())!;
        }

        public async Task<IReadOnlyList<CommentResponse>> GetCommentsAsync(string postId)
        {
            List<CommentResponse>? comments = await httpClient.GetFromJsonAsync<List<CommentResponse>>($"/api/posts/{Uri.EscapeDataString(postId)}/comments");
            return comments ?? [];
        }

        public async Task<CommentResponse> AddCommentAsync(string postId, string authorPublicId, string text)
        {
            HttpResponseMessage response = await httpClient.PostAsJsonAsync($"/api/posts/{Uri.EscapeDataString(postId)}/comments", new AddCommentRequest(authorPublicId, text));
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<CommentResponse>())!;
        }
    }
}
