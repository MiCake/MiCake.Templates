namespace StandardWeb.Application.Cache;

/// <summary>
/// A centralized place for cache key generation
/// </summary>
public static class CacheKeys
{
    /// <summary>
    /// returns a timestamped cache key
    /// </summary>
    public static string GetTimestampedKey(string baseKey, DateTime timestamp) => $"{baseKey}:ts:{timestamp:yyyyMMddHHmmss}";

    /// <summary>
    /// returns a versioned cache key
    /// </summary>
    public static string GetVersionedKey(string baseKey, string version) => $"{baseKey}:v:{version}";
}