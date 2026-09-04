using ChaySocialSonnet.MainProject.Constants.ThemeConstants;
using Microsoft.Maui.Graphics;

namespace ChaySocialSonnet.MainProject.Tests.ThemeConstants
{
    public class AppStylesTests
    {
        [Theory]
        [InlineData(AcrylicLevel.Subtle)]
        [InlineData(AcrylicLevel.Normal)]
        [InlineData(AcrylicLevel.Strong)]
        [InlineData(AcrylicLevel.TintPrimary)]
        [InlineData(AcrylicLevel.TintAccent)]
        public void BuildAcrylicStyle_EveryLevel_ProducesBackgroundAndBackdropFilter(AcrylicLevel level)
        {
            var style = AppStyles.BuildAcrylicStyle(level);

            Assert.Contains("background:", style);
            Assert.Contains("backdrop-filter:", style);
            Assert.Contains("border:", style);
        }

        [Fact]
        public void GetShadow_FormatsRgbaWithInvariantCultureDecimalSeparator()
        {
            var shadow = AppStyles.GetShadow(0, 4, 12, Colors.Black, 0.5f);

            Assert.Equal("0px 4px 12px rgba(0, 0, 0, 0.5)", shadow);
        }

        [Fact]
        public void BuildAuroraBackground_IncludesEveryConfiguredAuroraStop()
        {
            var background = AppStyles.BuildAuroraBackground();

            Assert.Equal(AppColors.AuroraStops.Length, System.Text.RegularExpressions.Regex.Matches(background, "radial-gradient").Count);
        }
    }
}
