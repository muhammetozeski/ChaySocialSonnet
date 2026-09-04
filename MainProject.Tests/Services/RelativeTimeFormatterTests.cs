using ChaySocialSonnet.MainProject.Services;

namespace ChaySocialSonnet.MainProject.Tests.Services
{
    public class RelativeTimeFormatterTests
    {
        static readonly DateTimeOffset Now = new(2026, 9, 4, 12, 0, 0, TimeSpan.Zero);

        [Theory]
        [InlineData(30, "just now")]
        [InlineData(90, "1m ago")]
        [InlineData(3600, "1h ago")]
        [InlineData(90000, "1d ago")]
        public void Format_VariousElapsedSeconds_ProducesExpectedLabel(int elapsedSeconds, string expected)
        {
            DateTimeOffset timestamp = Now.AddSeconds(-elapsedSeconds);

            string result = RelativeTimeFormatter.Format(timestamp, Now);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Format_MoreThanAWeekAgo_FallsBackToAbsoluteDate()
        {
            DateTimeOffset timestamp = Now.AddDays(-10);

            string result = RelativeTimeFormatter.Format(timestamp, Now);

            Assert.Equal(timestamp.ToLocalTime().ToString("MMM d"), result);
        }
    }
}
