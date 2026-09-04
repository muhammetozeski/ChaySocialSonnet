using ChaySocialSonnet.MainProject.Backend;

namespace ChaySocialSonnet.Web.Backend
{
    /// <summary> Maps the /api/notifications/* endpoints backing <see cref="MainProject.Services.NotificationsApiClient"/>. </summary>
    public static class NotificationEndpoints
    {
        public static void MapNotificationEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/notifications/{publicId}", async (string publicId, int count, INotificationStore notifications) =>
            {
                IReadOnlyList<AppNotification> results = await notifications.GetForUserAsync(publicId, count);
                return Results.Ok(results.Select(notification => new NotificationResponse(notification.Id, notification.ActorPublicId, notification.Kind, notification.SubjectPostId, notification.CreatedAt, notification.IsRead)));
            });

            app.MapGet("/api/notifications/{publicId}/unread-count", async (string publicId, INotificationStore notifications) =>
                Results.Ok(await notifications.GetUnreadCountAsync(publicId)));

            app.MapPost("/api/notifications/{publicId}/mark-read", async (string publicId, INotificationStore notifications) =>
            {
                await notifications.MarkAllReadAsync(publicId);
                return Results.Ok();
            });
        }
    }
}
