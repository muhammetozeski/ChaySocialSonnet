using ChaySocialSonnet.MainProject.Backend;
using System.Collections.Concurrent;

namespace ChaySocialSonnet.Web.Backend
{
    /// <summary> In-memory <see cref="IReportStore"/>. Lost on restart, same as the other Local* stores. </summary>
    public sealed class LocalReportStore : IReportStore
    {
        readonly ConcurrentDictionary<string, ContentReport> reports = new();

        public Task<ContentReport> SubmitAsync(string reporterPublicId, string targetType, string targetId, string reason)
        {
            var report = new ContentReport(Guid.NewGuid().ToString("n"), reporterPublicId, targetType, targetId, reason, DateTimeOffset.UtcNow);
            reports[report.Id] = report;
            return Task.FromResult(report);
        }
    }
}
