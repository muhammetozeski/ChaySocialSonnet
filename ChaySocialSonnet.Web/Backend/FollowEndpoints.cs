using ChaySocialSonnet.MainProject.Backend;
using Microsoft.AspNetCore.Mvc;

namespace ChaySocialSonnet.Web.Backend
{
    /// <summary> Maps the /api/follow/* endpoints backing <see cref="MainProject.Services.FollowApiClient"/>. </summary>
    public static class FollowEndpoints
    {
        public static void MapFollowEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/follow/{targetPublicId}", async (string targetPublicId, IFollowStore follows, INotificationStore notifications, [FromHeader(Name = "Authorization")] string? authorization, IIdentityRegistry registry) =>
            {
                string? actingPublicId = await RequestAuthentication.ResolveActingPublicIdAsync(authorization, registry);
                if (actingPublicId is null)
                {
                    return Results.Unauthorized();
                }

                await follows.FollowAsync(actingPublicId, targetPublicId);
                if (actingPublicId != targetPublicId)
                {
                    await notifications.AddAsync(targetPublicId, actingPublicId, NotificationKind.Follow, subjectPostId: null);
                }
                return Results.Ok(await BuildStatusAsync(targetPublicId, actingPublicId, follows));
            });

            app.MapDelete("/api/follow/{targetPublicId}", async (string targetPublicId, IFollowStore follows, [FromHeader(Name = "Authorization")] string? authorization, IIdentityRegistry registry) =>
            {
                string? actingPublicId = await RequestAuthentication.ResolveActingPublicIdAsync(authorization, registry);
                if (actingPublicId is null)
                {
                    return Results.Unauthorized();
                }

                await follows.UnfollowAsync(actingPublicId, targetPublicId);
                return Results.Ok(await BuildStatusAsync(targetPublicId, actingPublicId, follows));
            });

            app.MapGet("/api/follow/{targetPublicId}/status", async (string targetPublicId, string? viewerPublicId, IFollowStore follows) =>
                Results.Ok(await BuildStatusAsync(targetPublicId, viewerPublicId, follows)));

            app.MapGet("/api/follow/{publicId}/following-ids", async (string publicId, IFollowStore follows) =>
                Results.Ok(await follows.GetFollowingIdsAsync(publicId)));
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
