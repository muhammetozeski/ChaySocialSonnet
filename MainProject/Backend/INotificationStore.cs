namespace ChaySocialSonnet.MainProject.Backend
{
    public enum NotificationKind { Like, Comment, Follow }

    /// <summary> One "someone did something to your stuff" event. <see cref="SubjectPostId"/> is null for a Follow notification. </summary>
    public sealed record AppNotification(string Id, string RecipientPublicId, string ActorPublicId, NotificationKind Kind, string? SubjectPostId, DateTimeOffset CreatedAt, bool IsRead);

    /// <summary> Server-side storage for in-app notifications. </summary>
    public interface INotificationStore
    {
        Task AddAsync(string recipientPublicId, string actorPublicId, NotificationKind kind, string? subjectPostId);

        /// <summary> Notifications for <paramref name="recipientPublicId"/>, newest first. </summary>
        Task<IReadOnlyList<AppNotification>> GetForUserAsync(string recipientPublicId, int count);

        Task<int> GetUnreadCountAsync(string recipientPublicId);

        Task MarkAllReadAsync(string recipientPublicId);
    }
}
