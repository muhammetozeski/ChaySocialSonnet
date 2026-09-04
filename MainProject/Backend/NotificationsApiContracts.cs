namespace ChaySocialSonnet.MainProject.Backend
{
    public sealed record NotificationResponse(string Id, string ActorPublicId, NotificationKind Kind, string? SubjectPostId, DateTimeOffset CreatedAt, bool IsRead);
}
