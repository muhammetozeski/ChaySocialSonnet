using ChaySocialSonnet.MainProject.Backend;
using Microsoft.AspNetCore.Mvc;

namespace ChaySocialSonnet.Web.Backend
{
    /// <summary> Maps the /api/posts/* endpoints backing <see cref="MainProject.Services.PostApiClient"/>. Public posts are stored and served in the clear (see <see cref="PublicPost"/>'s own summary for why). </summary>
    public static class PostEndpoints
    {
        /// <summary> A post or comment beyond this length is rejected — plain abuse-resistance, not a design statement about ideal post length. </summary>
        const int MaxTextLength = 2000;

        public static void MapPostEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/posts/recent", async (int count, string? viewerPublicId, IPostStore posts, ILikeStore likes, ICommentStore comments, IBlockStore blocks) =>
            {
                IReadOnlyList<PublicPost> visible = await FilterBlockedAsync(await posts.GetRecentPostsAsync(count), viewerPublicId, blocks);
                return Results.Ok(await ToSummariesAsync(visible, viewerPublicId, likes, comments));
            });

            app.MapGet("/api/posts/by-author/{authorPublicId}", async (string authorPublicId, int count, string? viewerPublicId, IPostStore posts, ILikeStore likes, ICommentStore comments, IBlockStore blocks) =>
            {
                IReadOnlyList<PublicPost> visible = await FilterBlockedAsync(await posts.GetPostsByAuthorAsync(authorPublicId, count), viewerPublicId, blocks);
                return Results.Ok(await ToSummariesAsync(visible, viewerPublicId, likes, comments));
            });

            app.MapGet("/api/posts/{postId}", async (string postId, string? viewerPublicId, IPostStore posts, ILikeStore likes, ICommentStore comments, IBlockStore blocks) =>
            {
                PublicPost? post = await posts.GetByIdAsync(postId);
                if (post is null)
                {
                    return Results.NotFound();
                }

                IReadOnlyList<PublicPost> visible = await FilterBlockedAsync([post], viewerPublicId, blocks);
                PostSummary? summary = (await ToSummariesAsync(visible, viewerPublicId, likes, comments)).SingleOrDefault();
                return summary is null ? Results.NotFound() : Results.Ok(summary);
            });

            app.MapPost("/api/posts", async (CreatePostRequest request, IPostStore posts, [FromHeader(Name = "Authorization")] string? authorization, IIdentityRegistry registry) =>
            {
                string? actingPublicId = await RequestAuthentication.ResolveActingPublicIdAsync(authorization, registry);
                if (actingPublicId is null)
                {
                    return Results.Unauthorized();
                }
                if (!IsValidText(request.Text))
                {
                    return Results.BadRequest("text must be non-empty and at most " + MaxTextLength + " characters.");
                }

                PublicPost post = await posts.CreatePostAsync(actingPublicId, request.Text);
                return Results.Ok(new PostSummary(post.Id, post.AuthorPublicId, post.Text, post.CreatedAt, LikeCount: 0, CommentCount: 0, LikedByViewer: false));
            });

            app.MapDelete("/api/posts/{postId}", async (string postId, IPostStore posts, [FromHeader(Name = "Authorization")] string? authorization, IIdentityRegistry registry) =>
            {
                string? actingPublicId = await RequestAuthentication.ResolveActingPublicIdAsync(authorization, registry);
                if (actingPublicId is null)
                {
                    return Results.Unauthorized();
                }
                return await posts.DeletePostAsync(postId, actingPublicId) ? Results.Ok() : Results.NotFound();
            });

            app.MapPost("/api/posts/{postId}/like", async (string postId, ILikeStore likes, IPostStore posts, INotificationStore notifications, IBlockStore blocks, [FromHeader(Name = "Authorization")] string? authorization, IIdentityRegistry registry) =>
            {
                string? actingPublicId = await RequestAuthentication.ResolveActingPublicIdAsync(authorization, registry);
                if (actingPublicId is null)
                {
                    return Results.Unauthorized();
                }

                PublicPost? post = await posts.GetByIdAsync(postId);
                if (post is null || await IsBlockedEitherWayAsync(actingPublicId, post.AuthorPublicId, blocks))
                {
                    return Results.NotFound();
                }

                bool liked = await likes.ToggleLikeAsync(postId, actingPublicId);
                int count = await likes.GetLikeCountAsync(postId);

                if (liked && post.AuthorPublicId != actingPublicId)
                {
                    await notifications.AddAsync(post.AuthorPublicId, actingPublicId, NotificationKind.Like, postId);
                }

                return Results.Ok(new ToggleLikeResponse(liked, count));
            });

            app.MapGet("/api/posts/{postId}/comments", async (string postId, string? viewerPublicId, IPostStore posts, ICommentStore comments, IBlockStore blocks) =>
            {
                PublicPost? post = await posts.GetByIdAsync(postId);
                if (post is not null && viewerPublicId is not null && await IsBlockedEitherWayAsync(viewerPublicId, post.AuthorPublicId, blocks))
                {
                    return Results.Ok(Array.Empty<CommentResponse>());
                }

                IReadOnlyList<PostComment> postComments = await comments.GetCommentsAsync(postId);
                if (viewerPublicId is not null)
                {
                    var visible = new List<PostComment>(postComments.Count);
                    foreach (PostComment comment in postComments)
                    {
                        if (!await IsBlockedEitherWayAsync(viewerPublicId, comment.AuthorPublicId, blocks))
                        {
                            visible.Add(comment);
                        }
                    }
                    postComments = visible;
                }

                return Results.Ok(postComments.Select(comment => new CommentResponse(comment.Id, comment.AuthorPublicId, comment.Text, comment.CreatedAt)));
            });

            app.MapPost("/api/posts/{postId}/comments", async (string postId, AddCommentRequest request, ICommentStore comments, IPostStore posts, INotificationStore notifications, IBlockStore blocks, [FromHeader(Name = "Authorization")] string? authorization, IIdentityRegistry registry) =>
            {
                string? actingPublicId = await RequestAuthentication.ResolveActingPublicIdAsync(authorization, registry);
                if (actingPublicId is null)
                {
                    return Results.Unauthorized();
                }
                if (!IsValidText(request.Text))
                {
                    return Results.BadRequest("text must be non-empty and at most " + MaxTextLength + " characters.");
                }

                PublicPost? post = await posts.GetByIdAsync(postId);
                if (post is null || await IsBlockedEitherWayAsync(actingPublicId, post.AuthorPublicId, blocks))
                {
                    return Results.NotFound();
                }

                PostComment comment = await comments.AddCommentAsync(postId, actingPublicId, request.Text);
                if (post.AuthorPublicId != actingPublicId)
                {
                    await notifications.AddAsync(post.AuthorPublicId, actingPublicId, NotificationKind.Comment, postId);
                }
                return Results.Ok(new CommentResponse(comment.Id, comment.AuthorPublicId, comment.Text, comment.CreatedAt));
            });
        }

        static bool IsValidText(string text) => !string.IsNullOrWhiteSpace(text) && text.Length <= MaxTextLength;

        /// <summary> True if either identity has blocked the other — the shared "can these two interact" check for every read filter and every mutating endpoint in this file. </summary>
        internal static async Task<bool> IsBlockedEitherWayAsync(string a, string b, IBlockStore blocks) =>
            await blocks.IsBlockedAsync(a, b) || await blocks.IsBlockedAsync(b, a);

        /// <summary> Drops posts where the viewer has blocked the author or the author has blocked the viewer, in either direction. A no-op for an anonymous (null) viewer. </summary>
        static async Task<IReadOnlyList<PublicPost>> FilterBlockedAsync(IReadOnlyList<PublicPost> posts, string? viewerPublicId, IBlockStore blocks)
        {
            if (viewerPublicId is null)
            {
                return posts;
            }

            var visible = new List<PublicPost>(posts.Count);
            foreach (PublicPost post in posts)
            {
                if (!await IsBlockedEitherWayAsync(viewerPublicId, post.AuthorPublicId, blocks))
                {
                    visible.Add(post);
                }
            }
            return visible;
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
