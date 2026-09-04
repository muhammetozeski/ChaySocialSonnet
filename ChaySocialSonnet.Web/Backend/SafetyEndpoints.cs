using ChaySocialSonnet.MainProject.Backend;
using Microsoft.AspNetCore.Mvc;

namespace ChaySocialSonnet.Web.Backend
{
    /// <summary> Maps the /api/block/* and /api/report endpoints backing <see cref="MainProject.Services.SafetyApiClient"/>. </summary>
    public static class SafetyEndpoints
    {
        public static void MapSafetyEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/block/{blockedPublicId}", async (string blockedPublicId, BlockRequest request, IBlockStore blocks) =>
            {
                await blocks.BlockAsync(request.BlockerPublicId, blockedPublicId);
                return Results.Ok();
            });

            app.MapDelete("/api/block/{blockedPublicId}", async (string blockedPublicId, [FromBody] BlockRequest request, IBlockStore blocks) =>
            {
                await blocks.UnblockAsync(request.BlockerPublicId, blockedPublicId);
                return Results.Ok();
            });

            app.MapGet("/api/block/{blockedPublicId}/status", async (string blockedPublicId, string blockerPublicId, IBlockStore blocks) =>
                Results.Ok(await blocks.IsBlockedAsync(blockerPublicId, blockedPublicId)));

            app.MapPost("/api/report", async (SubmitReportRequest request, IReportStore reports) =>
            {
                await reports.SubmitAsync(request.ReporterPublicId, request.TargetType, request.TargetId, request.Reason);
                return Results.Ok();
            });
        }
    }
}
