using ChaySocialSonnet.MainProject.Backend;
using System.Collections.Concurrent;

namespace ChaySocialSonnet.Web.Backend
{
    /// <summary> In-memory <see cref="INotificationStore"/>. Lost on restart, same as the other Local* stores. </summary>
    public sealed class LocalNotificationStore : INotificationStore
    {
        readonly ConcurrentDictionary<string, AppNotification> notifications = new();

        public Task AddAsync(string recipientPublicId, string actorPublicId, NotificationKind kind, string? subjectPostId)
        {
            var notification = new AppNotification(Guid.NewGuid().ToString("n"), recipientPublicId, actorPublicId, kind, subjectPostId, DateTimeOffset.UtcNow, IsRead: false);
            notifications[notification.Id] = notification;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<AppNotification>> GetForUserAsync(string recipientPublicId, int count)
        {
            IReadOnlyList<AppNotification> results = notifications.Values
                .Where(notification => notification.RecipientPublicId == recipientPublicId)
                .OrderByDescending(notification => notification.CreatedAt)
                .Take(count)
                .ToList();
            return Task.FromResult(results);
        }

        public Task<int> GetUnreadCountAsync(string recipientPublicId) =>
            Task.FromResult(notifications.Values.Count(notification => notification.RecipientPublicId == recipientPublicId && !notification.IsRead));

        public Task MarkAllReadAsync(string recipientPublicId)
        {
            foreach (AppNotification notification in notifications.Values.Where(notification => notification.RecipientPublicId == recipientPublicId && !notification.IsRead))
            {
                notifications[notification.Id] = notification with { IsRead = true };
            }
            return Task.CompletedTask;
        }
    }
}
