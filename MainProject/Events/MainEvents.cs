namespace ChaySocialSonnet.MainProject.Events
{
    /// <summary>
    /// Central hub for app-wide notifications that do not belong to a single service.
    /// </summary>
    public static class MainEvents
    {
        public static readonly AppEvent OnThemeChanged = new();
    }
}
