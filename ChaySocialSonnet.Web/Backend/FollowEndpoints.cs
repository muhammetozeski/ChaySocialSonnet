using ChaySocialSonnet.MainProject.Backend;

namespace ChaySocialSonnet.Web.Backend
{
    /// <summary> Maps the /api/follow/* endpoints backing <see cref="MainProject.Services.FollowApiClient"/>. </summary>
    public static class FollowEndpoints
    {
        public static void MapFollowEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/follow/{targetPublicId}", async (string targetPublicId, FollowRequest request, IFollowStore follows, INotificationStore notifications) =>
            {
                await follows.FollowAsync(request.FollowerPublicId, targetPublicId);
                if (request.FollowerPublicId != targetPublicId)
                {
                    await notifications.AddAsync(targetPublicId, request.FollowerPublicId, NotificationKind.Follow, subjectPostId: null);
                }
                return Results.Ok(await BuildStatusAsync(targetPublicId, request.FollowerPublicId, follows));
            });

            app.MapDelete("/api/follow/{targetPublicId}", async (string targetPublicId, FollowRequest request, IFollowStore follows) =>
            {
                await follows.UnfollowAsync(request.FollowerPublicId, targetPublicId);
                return Results.Ok(await BuildStatusAsync(targetPublicId, request.FollowerPublicId, follows));
            });

            app.MapGet("/api/follow/{targetPublicId}/status", async (string targetPublicId, string? viewerPublicId, IFollowStore follows) =>
                Results.Ok(await BuildStatusAsync(targetPublicId, viewerPublicId, follows)));
        }

        static async Task<FollowStatusResponse> BuildStatusAsync(string targetPublicId, string? viewerPublicId, IFollowStore follows)
        {
            bool isFollowing = viewerPublicId is not null && await follows.IsFollowingAsync(viewerPublicId, targetPublicId);
            int followerCount = await follows.GetFollowerCountAsync(targetPublicId);
            int followingCount = await follows.GetFollowingCountAsync(targetPublicId);
            return new FollowStatusResponse(isFollowing, followerCount, followingCount);
        }
    }
}
