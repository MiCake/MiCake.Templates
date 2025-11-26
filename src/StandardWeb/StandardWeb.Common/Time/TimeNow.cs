namespace StandardWeb.Common.Time;

/// <summary>
/// Provides a centralized access point for current time.
/// Useful for testing and time zone management across the application.
/// </summary>
/// <remarks>
/// Using this class instead of DateTime.Now directly enables easier unit testing
/// by allowing time to be mocked or controlled in test scenarios.
/// </remarks>
public class TimeNow
{
    /// <summary>
    /// Gets the current local date and time.
    /// </summary>
    public static DateTime Now { get => DateTime.UtcNow; }
}
