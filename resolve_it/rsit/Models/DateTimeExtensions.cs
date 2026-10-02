namespace rsit;

public static class DateTimeExtensions
{
    private static readonly TimeZoneInfo DisplayZone = ResolveZone();

    private static TimeZoneInfo ResolveZone()
    {
        foreach (var id in new[] { "India Standard Time", "Asia/Kolkata" })
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById(id); } catch { }
        }
        return TimeZoneInfo.Local;
    }

    public static DateTime ToDisplayTime(this DateTime value)
    {
        var utc = value.Kind == DateTimeKind.Local
            ? value.ToUniversalTime()
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(utc, DisplayZone);
    }

    public static DateTime? ToDisplayTime(this DateTime? value) =>
        value?.ToDisplayTime();

    public static string ToDisplayDate(this DateTime value) =>
        value.ToDisplayTime().ToString("dd MMM yyyy");

    public static string ToDisplayClock(this DateTime value) =>
        value.ToDisplayTime().ToString("hh:mm tt");

    public static string ToDisplayDateTime(this DateTime value) =>
        value.ToDisplayTime().ToString("dd MMM yyyy, hh:mm tt");
}
