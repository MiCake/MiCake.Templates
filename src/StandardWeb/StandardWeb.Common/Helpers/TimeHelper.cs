namespace StandardWeb.Common.Helpers;

/// <summary>
/// Provides time-related utility methods for timestamp conversion and validation.
/// </summary>
public static class TimeHelper
{
    /// <summary>
    /// Converts a DateTime to Unix timestamp (seconds or milliseconds since 1970-01-01 UTC).
    /// </summary>
    /// <param name="dateTime">DateTime to convert</param>
    /// <param name="type">Output format: Seconds or Milliseconds</param>
    /// <returns>Unix timestamp as string</returns>
    public static string GetUnixTimeStamp(DateTime dateTime, UnixTimeStampType type = UnixTimeStampType.Seconds)
    {
        var unixTime = (dateTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        return type == UnixTimeStampType.Milliseconds ? (unixTime * 1000).ToString() : unixTime.ToString();
    }

    /// <summary>
    /// Validates that start time is not greater than end time.
    /// Useful for date range validation in queries and filters.
    /// </summary>
    /// <param name="startTime">Start of the time range</param>
    /// <param name="endTime">Optional end of the time range</param>
    /// <exception cref="ArgumentException">Thrown when start time is after end time</exception>
    public static void CheckTimeRange(DateTime startTime, DateTime? endTime)
    {
        if (endTime.HasValue && startTime > endTime.Value)
        {
            throw new ArgumentException("Start time cannot be greater than end time.");
        }
    }

    /// <summary>
    /// Specifies the precision of Unix timestamp output.
    /// </summary>
    public enum UnixTimeStampType
    {
        /// <summary>
        /// Seconds since Unix epoch (1970-01-01 00:00:00 UTC)
        /// </summary>
        Seconds,

        /// <summary>
        /// Milliseconds since Unix epoch (for higher precision)
        /// </summary>
        Milliseconds
    }
}
