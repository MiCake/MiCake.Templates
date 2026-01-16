namespace StandardWeb.Application.Cache;

/// <summary>
/// A centralized place for cache key generation
/// </summary>
public static class CacheKeys
{
    // ==================== Configuration Module ====================

    /// <summary>
    /// Cache key for individual AppSetting: "AppSetting:{SettingGroup}:{Key}"
    /// Example: AppSetting:Email:SmtpServer
    /// </summary>
    public const string AppSettingByKey = "AppSetting:{0}:{1}";

    // ==================== Other Modules ====================

    // Example cache keys: 
    // public const string UserById = "User:{0}";

    /// <summary>
    /// returns a timestamped cache key
    /// </summary>
    public static string GetTimestampedKey(string baseKey, DateTime timestamp) => $"{baseKey}:ts:{timestamp:yyyyMMddHHmmss}";

    /// <summary>
    /// returns a versioned cache key
    /// </summary>
    public static string GetVersionedKey(string baseKey, string version) => $"{baseKey}:v:{version}";
}