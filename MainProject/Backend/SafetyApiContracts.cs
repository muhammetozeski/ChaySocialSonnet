namespace ChaySocialSonnet.MainProject.Backend
{
    public sealed record BlockRequest(string BlockerPublicId);

    public sealed record SubmitReportRequest(string ReporterPublicId, string TargetType, string TargetId, string Reason);
}
