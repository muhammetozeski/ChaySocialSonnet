namespace ChaySocialSonnet.MainProject.Backend
{
    /// <summary> The reporter is the caller resolved server-side from their session token — never a field in this request. </summary>
    public sealed record SubmitReportRequest(string TargetType, string TargetId, string Reason);
}
