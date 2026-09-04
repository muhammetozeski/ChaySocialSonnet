namespace ChaySocialSonnet.MainProject.Backend
{
    /// <summary> A user's report of a post or another user, for later moderation review. </summary>
    public sealed record ContentReport(string Id, string ReporterPublicId, string TargetType, string TargetId, string Reason, DateTimeOffset CreatedAt);

    /// <summary> Server-side storage for content/user reports. Nothing reviews these automatically yet — this only captures them so a moderation flow has something to work from later. </summary>
    public interface IReportStore
    {
        Task<ContentReport> SubmitAsync(string reporterPublicId, string targetType, string targetId, string reason);
    }
}
