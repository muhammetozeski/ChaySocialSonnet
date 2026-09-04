namespace ChaySocialSonnet.MainProject.Services
{
    /// <summary> Formats a timestamp as a short "time ago" string (e.g. "3m ago", "2h ago", "5d ago") relative to a given reference time. </summary>
    public static class RelativeTimeFormatter
    {
        public static string Format(DateTimeOffset timestamp, DateTimeOffset now)
        {
            TimeSpan elapsed = now - timestamp;
            return elapsed switch
            {
                { TotalSeconds: < 60 } => "just now",
                { TotalMinutes: < 60 } => $"{(int)elapsed.TotalMinutes}m ago",
                { TotalHours: < 24 } => $"{(int)elapsed.TotalHours}h ago",
                { TotalDays: < 7 } => $"{(int)elapsed.TotalDays}d ago",
                _ => timestamp.ToLocalTime().ToString("MMM d")
            };
        }
    }
}
