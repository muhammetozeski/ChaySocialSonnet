using ChaySocialSonnet.MainProject.Backend;
using Microsoft.AspNetCore.Mvc;

namespace ChaySocialSonnet.Web.Backend
{
    /// <summary> Maps the /api/block/* and /api/report endpoints backing <see cref="MainProject.Services.SafetyApiClient"/>. </summary>
    public static class SafetyEndpoints
    {
        public static void MapSafetyEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/block/{blockedPublicId}", async (string blockedPublicId, IBlockStore blocks, [FromHeader(Name = "Authorization")] string? authorization, IIdentityRegistry registry) =>
            {
                string? actingPublicId = await RequestAuthentication.ResolveActingPublicIdAsync(authorization, registry);
                if (actingPublicId is null)
                {
                    return Results.Unauthorized();
                }
                await blocks.BlockAsync(actingPublicId, blockedPublicId);
                return Results.Ok();
            });

            app.MapDelete("/api/block/{blockedPublicId}", async (string blockedPublicId, IBlockStore blocks, [FromHeader(Name = "Authorization")] string? authorization, IIdentityRegistry registry) =>
            {
                string? actingPublicId = await RequestAuthentication.ResolveActingPublicIdAsync(authorization, registry);
                if (actingPublicId is null)
                {
                    return Results.Unauthorized();
                }
                await blocks.UnblockAsync(actingPublicId, blockedPublicId);
                return Results.Ok();
            });

            app.MapGet("/api/block/{blockedPublicId}/status", async (string blockedPublicId, string blockerPublicId, IBlockStore blocks) =>
                Results.Ok(await blocks.IsBlockedAsync(blockerPublicId, blockedPublicId)));

            app.MapPost("/api/report", async (SubmitReportRequest request, IReportStore reports, [FromHeader(Name = "Authorization")] string? authorization, IIdentityRegistry registry) =>
            {
                string? actingPublicId = await RequestAuthentication.ResolveActingPublicIdAsync(authorization, registry);
                if (actingPublicId is null)
                {
                    return Results.Unauthorized();
                }
                await reports.SubmitAsync(actingPublicId, request.TargetType, request.TargetId, request.Reason);
                return Results.Ok();
            });
        }
    }
}
