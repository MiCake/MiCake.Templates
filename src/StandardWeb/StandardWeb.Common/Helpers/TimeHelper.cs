namespace StandardWeb.Common.Helpers;

public static class TimeHelper
{
    public static string GetUnixTimeStamp(DateTime dateTime, UnixTimeStampType type = UnixTimeStampType.Seconds)
    {
        var unixTime = (dateTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        return type == UnixTimeStampType.Milliseconds ? (unixTime * 1000).ToString() : unixTime.ToString();
    }

    public static void CheckTimeRange(DateTime startTime, DateTime? endTime)
    {
        if (endTime.HasValue && startTime > endTime.Value)
        {
            throw new ArgumentException("Start time cannot be greater than end time.");
        }
    }

    public enum UnixTimeStampType
    {
        Seconds,
        Milliseconds
    }
}
