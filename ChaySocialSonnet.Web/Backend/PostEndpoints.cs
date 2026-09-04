using ChaySocialSonnet.MainProject.Backend;

namespace ChaySocialSonnet.Web.Backend
{
    /// <summary> Maps the /api/posts/* endpoints backing <see cref="MainProject.Services.PostApiClient"/>. Public posts are stored and served in the clear (see <see cref="PublicPost"/>'s own summary for why). </summary>
    public static class PostEndpoints
    {
        public static void MapPostEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/posts/recent", async (int count, IPostStore store) =>
                Results.Ok(await store.GetRecentPostsAsync(count)));
        }
    }
}
