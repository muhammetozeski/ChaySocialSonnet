using Microsoft.AspNetCore.Components;

namespace ChaySocialSonnet.MainProject.Constants
{
    /// <summary>
    /// Shared helpers for matching the browser's current URI against <c>NavigationConstants</c> entries
    /// (the source-generated class from <see cref="NavigationData"/>). Used by both
    /// <c>MainLayout</c> (to decide whether the bottom nav should show at all) and
    /// <c>AppBottomNav</c> (to decide which tab is active), so the two never drift out of sync.
    /// </summary>
    public static class NavigationRouting
    {
        /// <summary> Converts a raw <see cref="NavigationManager"/> URI into an absolute, lowercase, base-relative path (e.g. "/home") ready to match against a <c>NavigationConstants</c> link. </summary>
        public static string ToAbsoluteLowercasePath(NavigationManager navManager, string uri) =>
            "/" + navManager.ToBaseRelativePath(uri).ToLowerInvariant();

        /// <summary> True if <paramref name="absolutePath"/> matches one of the app's bottom-navigation destinations (Home, Search, Messages, Notifications, Profile). </summary>
        public static bool IsBottomNavigationRoute(string absolutePath) =>
            NavigationConstants.BottomNavigationItems.Any(item => absolutePath.StartsWith(item.Link.ToLowerInvariant()));
    }
}
