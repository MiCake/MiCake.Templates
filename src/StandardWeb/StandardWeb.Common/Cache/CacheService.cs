using MiCake.Core.DependencyInjection;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StandardWeb.Application.Cache;

[InjectService(ServiceTypes = [typeof(ICacheService)], Lifetime = MiCakeServiceLifetime.Scoped)]
public class CacheService(IDistributedCache distributedCache, ILogger<CacheService> logger) : ICacheService
{
    private readonly IDistributedCache _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
    private readonly ILogger<CacheService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private readonly static JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        WriteIndented = false
    };

    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(30);

    /// <inheritdoc />
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        try
        {
            var cachedValue = await _distributedCache.GetStringAsync(key, cancellationToken);

            if (string.IsNullOrEmpty(cachedValue))
            {
                _logger.LogDebug("Cache miss for key: {CacheKey}", key);
                return default;
            }

            var result = JsonSerializer.Deserialize<T>(cachedValue, _jsonOptions);
            _logger.LogDebug("Cache hit for key: {CacheKey}", key);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache item with key: {CacheKey}", key);
            return default;
        }
    }

    /// <inheritdoc />
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        if (value == null)
        {
            await RemoveAsync(key, cancellationToken);
            return;
        }

        try
        {
            var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? DefaultExpiration
            };

            await _distributedCache.SetStringAsync(key, serializedValue, options, cancellationToken);
            _logger.LogDebug("Cache set for key: {CacheKey}, expiration: {Expiration}", key, expiration ?? DefaultExpiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache item with key: {CacheKey}", key);
        }
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        try
        {
            await _distributedCache.RemoveAsync(key, cancellationToken);
            _logger.LogDebug("Cache removed for key: {CacheKey}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache item with key: {CacheKey}", key);
        }
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        try
        {
            var value = await _distributedCache.GetAsync(key, cancellationToken);
            return value != null && value.Length > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking cache existence for key: {CacheKey}", key);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T?>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(factory);

        var cachedValue = await GetAsync<T>(key, cancellationToken);
        if (cachedValue != null)
        {
            return cachedValue;
        }

        _logger.LogDebug("Cache miss for key: {CacheKey}, executing factory method", key);
        try
        {
            var newValue = await factory();

            if (newValue != null)
            {
                await SetAsync(key, newValue, expiration, cancellationToken);
            }

            return newValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing factory method for key: {CacheKey}", key);
            return default;
        }
    }

    /// <inheritdoc />
    public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prefix);

        try
        {

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache items with prefix: {Prefix}", prefix);
            return Task.CompletedTask;
        }
    }

    /// <inheritdoc />
    public T? Get<T>(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        try
        {
            var cachedValue = _distributedCache.GetString(key);

            if (string.IsNullOrEmpty(cachedValue))
            {
                _logger.LogDebug("Cache miss for key: {CacheKey}", key);
                return default;
            }

            var result = JsonSerializer.Deserialize<T>(cachedValue, _jsonOptions);
            _logger.LogDebug("Cache hit for key: {CacheKey}", key);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache item with key: {CacheKey}", key);
            return default;
        }
    }

    /// <inheritdoc />
    public void Set<T>(string key, T value, TimeSpan? expiration = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        if (value == null)
        {
            Remove(key);
            return;
        }

        try
        {
            var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? DefaultExpiration
            };

            _distributedCache.SetString(key, serializedValue, options);
            _logger.LogDebug("Cache set for key: {CacheKey}, expiration: {Expiration}", key, expiration ?? DefaultExpiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache item with key: {CacheKey}", key);
        }
    }

    /// <inheritdoc />
    public void Remove(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        try
        {
            _distributedCache.Remove(key);
            _logger.LogDebug("Cache removed for key: {CacheKey}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache item with key: {CacheKey}", key);
        }
    }

    /// <inheritdoc />
    public bool Exists(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        try
        {
            var value = _distributedCache.GetString(key);
            return !string.IsNullOrEmpty(value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking cache existence for key: {CacheKey}", key);
            return false;
        }
    }

    /// <inheritdoc />
    public T? GetOrSet<T>(string key, Func<T?> factory, TimeSpan? expiration = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(factory);

        var cachedValue = Get<T>(key);
        if (cachedValue != null)
        {
            return cachedValue;
        }

        _logger.LogDebug("Cache miss for key: {CacheKey}, executing factory method", key);
        try
        {
            var newValue = factory();

            if (newValue != null)
            {
                Set(key, newValue, expiration);
            }

            return newValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing factory method for key: {CacheKey}", key);
            return default;
        }
    }
}