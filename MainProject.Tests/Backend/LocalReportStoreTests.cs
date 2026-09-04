using ChaySocialSonnet.MainProject.Backend;
using ChaySocialSonnet.Web.Backend;

namespace ChaySocialSonnet.MainProject.Tests.Backend
{
    public class LocalReportStoreTests
    {
        [Fact]
        public async Task SubmitAsync_ReturnsAReportCarryingTheGivenDetails()
        {
            var store = new LocalReportStore();

            ContentReport report = await store.SubmitAsync("alice", "post", "post1", "spam");

            Assert.Equal("alice", report.ReporterPublicId);
            Assert.Equal("post", report.TargetType);
            Assert.Equal("post1", report.TargetId);
            Assert.Equal("spam", report.Reason);
            Assert.NotEmpty(report.Id);
        }

        [Fact]
        public async Task SubmitAsync_CalledTwice_ProducesTwoDistinctReportIds()
        {
            var store = new LocalReportStore();

            ContentReport first = await store.SubmitAsync("alice", "post", "post1", "spam");
            ContentReport second = await store.SubmitAsync("alice", "post", "post1", "spam");

            Assert.NotEqual(first.Id, second.Id);
        }
    }
}
