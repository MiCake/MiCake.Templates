namespace StandardWeb.Common.Time;

/// <summary>
/// Provides a centralized access point for current time.
/// Useful for testing and time zone management across the application.
/// </summary>
public static class TimeNow
{
    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    public static DateTimeOffset Now { get => DateTimeOffset.UtcNow; }
}
