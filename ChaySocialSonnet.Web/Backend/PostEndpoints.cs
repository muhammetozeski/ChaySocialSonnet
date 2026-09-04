using ChaySocialSonnet.MainProject.Backend;

namespace ChaySocialSonnet.Web.Backend
{
    /// <summary> Maps the /api/posts/* endpoints backing <see cref="MainProject.Services.PostApiClient"/>. Public posts are stored and served in the clear (see <see cref="PublicPost"/>'s own summary for why). </summary>
    public static class PostEndpoints
    {
        public static void MapPostEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/posts/recent", async (int count, string? viewerPublicId, IPostStore posts, ILikeStore likes, ICommentStore comments) =>
                Results.Ok(await ToSummariesAsync(await posts.GetRecentPostsAsync(count), viewerPublicId, likes, comments)));

            app.MapGet("/api/posts/by-author/{authorPublicId}", async (string authorPublicId, int count, string? viewerPublicId, IPostStore posts, ILikeStore likes, ICommentStore comments) =>
                Results.Ok(await ToSummariesAsync(await posts.GetPostsByAuthorAsync(authorPublicId, count), viewerPublicId, likes, comments)));

            app.MapPost("/api/posts", async (CreatePostRequest request, IPostStore posts) =>
            {
                PublicPost post = await posts.CreatePostAsync(request.AuthorPublicId, request.Text);
                return Results.Ok(new PostSummary(post.Id, post.AuthorPublicId, post.Text, post.CreatedAt, LikeCount: 0, CommentCount: 0, LikedByViewer: false));
            });

            app.MapDelete("/api/posts/{postId}", async (string postId, DeletePostRequest request, IPostStore posts) =>
                await posts.DeletePostAsync(postId, request.RequestingPublicId) ? Results.Ok() : Results.NotFound());

            app.MapPost("/api/posts/{postId}/like", async (string postId, ToggleLikeRequest request, ILikeStore likes, IPostStore posts, INotificationStore notifications) =>
            {
                bool liked = await likes.ToggleLikeAsync(postId, request.LikerPublicId);
                int count = await likes.GetLikeCountAsync(postId);

                if (liked)
                {
                    await NotifyPostAuthorAsync(postId, request.LikerPublicId, NotificationKind.Like, posts, notifications);
                }

                return Results.Ok(new ToggleLikeResponse(liked, count));
            });

            app.MapGet("/api/posts/{postId}/comments", async (string postId, ICommentStore comments) =>
            {
                IReadOnlyList<PostComment> postComments = await comments.GetCommentsAsync(postId);
                return Results.Ok(postComments.Select(comment => new CommentResponse(comment.Id, comment.AuthorPublicId, comment.Text, comment.CreatedAt)));
            });

            app.MapPost("/api/posts/{postId}/comments", async (string postId, AddCommentRequest request, ICommentStore comments, IPostStore posts, INotificationStore notifications) =>
            {
                PostComment comment = await comments.AddCommentAsync(postId, request.AuthorPublicId, request.Text);
                await NotifyPostAuthorAsync(postId, request.AuthorPublicId, NotificationKind.Comment, posts, notifications);
                return Results.Ok(new CommentResponse(comment.Id, comment.AuthorPublicId, comment.Text, comment.CreatedAt));
            });
        }

        static async Task NotifyPostAuthorAsync(string postId, string actorPublicId, NotificationKind kind, IPostStore posts, INotificationStore notifications)
        {
            PublicPost? post = await posts.GetByIdAsync(postId);
            if (post is not null && post.AuthorPublicId != actorPublicId)
            {
                await notifications.AddAsync(post.AuthorPublicId, actorPublicId, kind, postId);
            }
        }

        static async Task<IEnumerable<PostSummary>> ToSummariesAsync(IReadOnlyList<PublicPost> posts, string? viewerPublicId, ILikeStore likes, ICommentStore comments)
        {
            var summaries = new List<PostSummary>(posts.Count);
            foreach (PublicPost post in posts)
            {
                int likeCount = await likes.GetLikeCountAsync(post.Id);
                int commentCount = await comments.GetCommentCountAsync(post.Id);
                bool likedByViewer = viewerPublicId is not null && await likes.HasLikedAsync(post.Id, viewerPublicId);
                summaries.Add(new PostSummary(post.Id, post.AuthorPublicId, post.Text, post.CreatedAt, likeCount, commentCount, likedByViewer));
            }
            return summaries;
        }
    }
}
