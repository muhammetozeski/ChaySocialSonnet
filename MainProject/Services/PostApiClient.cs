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

        /// <summary> Creates a post as the current session's identity (see <see cref="AuthService.SessionToken"/>) — there is no "post as someone else" option. </summary>
        public async Task<PostSummary> CreatePostAsync(string text)
        {
            HttpRequestMessage request = AuthorizedRequests.Create(HttpMethod.Post, "/api/posts", new CreatePostRequest(text));
            HttpResponseMessage response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<PostSummary>())!;
        }

        public async Task DeletePostAsync(string postId)
        {
            HttpRequestMessage request = AuthorizedRequests.Create(HttpMethod.Delete, $"/api/posts/{Uri.EscapeDataString(postId)}");
            HttpResponseMessage response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        public async Task<ToggleLikeResponse> ToggleLikeAsync(string postId)
        {
            HttpRequestMessage request = AuthorizedRequests.Create(HttpMethod.Post, $"/api/posts/{Uri.EscapeDataString(postId)}/like");
            HttpResponseMessage response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<ToggleLikeResponse>())!;
        }

        public async Task<IReadOnlyList<CommentResponse>> GetCommentsAsync(string postId, string? viewerPublicId)
        {
            string viewerQuery = viewerPublicId is null ? "" : $"?viewerPublicId={Uri.EscapeDataString(viewerPublicId)}";
            List<CommentResponse>? comments = await httpClient.GetFromJsonAsync<List<CommentResponse>>($"/api/posts/{Uri.EscapeDataString(postId)}/comments{viewerQuery}");
            return comments ?? [];
        }

        public async Task<CommentResponse> AddCommentAsync(string postId, string text)
        {
            HttpRequestMessage request = AuthorizedRequests.Create(HttpMethod.Post, $"/api/posts/{Uri.EscapeDataString(postId)}/comments", new AddCommentRequest(text));
            HttpResponseMessage response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<CommentResponse>())!;
        }
    }
}
