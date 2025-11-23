namespace StandardWeb.Application.Cache;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    Task<T?> GetOrSetAsync<T>(string key, Func<Task<T?>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);

    T? Get<T>(string key);

    void Set<T>(string key, T value, TimeSpan? expiration = null);

    void Remove(string key);

    bool Exists(string key);

    T? GetOrSet<T>(string key, Func<T?> factory, TimeSpan? expiration = null);
}