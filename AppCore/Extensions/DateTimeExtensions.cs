public static class DateTimeExtensions
{
    public static DateTime RoundToNearestHour(this DateTime dt)
    {
        return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0).AddHours(1);
    }
} 