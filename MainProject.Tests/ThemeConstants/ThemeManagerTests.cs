using ChaySocialSonnet.MainProject.Constants.ThemeConstants;
using ChaySocialSonnet.MainProject.Events;

namespace ChaySocialSonnet.MainProject.Tests.ThemeConstants
{
    public class ThemeManagerTests
    {
        [Fact]
        public void Apply_DifferentTheme_UpdatesCurrentAndRaisesOnThemeChanged()
        {
            var originalTheme = ThemeManager.Current;
            var testTheme = originalTheme with { Name = "Test Theme" };
            var raiseCount = 0;
            void Handler() => raiseCount++;
            MainEvents.OnThemeChanged.Subscribe(Handler);

            try
            {
                ThemeManager.Apply(testTheme);

                Assert.Equal(testTheme, ThemeManager.Current);
                Assert.Equal(1, raiseCount);
            }
            finally
            {
                MainEvents.OnThemeChanged.Unsubscribe(Handler);
                ThemeManager.Apply(originalTheme);
            }
        }

        [Fact]
        public void Apply_SameThemeAlreadyCurrent_DoesNotRaiseOnThemeChanged()
        {
            var originalTheme = ThemeManager.Current;
            var raiseCount = 0;
            void Handler() => raiseCount++;
            MainEvents.OnThemeChanged.Subscribe(Handler);

            try
            {
                ThemeManager.Apply(originalTheme);

                Assert.Equal(0, raiseCount);
            }
            finally
            {
                MainEvents.OnThemeChanged.Unsubscribe(Handler);
            }
        }
    }
}
