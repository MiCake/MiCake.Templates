using RBACWeb.Application.Cache;
using RBACWeb.Domain.Enums.Authorization;
using RBACWeb.Domain.Models.Authorization;
using RBACWeb.Domain.Repositories;

namespace RBACWeb.Application.Authorization;

/// <summary>
/// Implementation of data scope resolution with caching support.
/// </summary>
[InjectService(typeof(IDataScopeResolver), Lifetime = MiCakeServiceLifetime.Scoped)]
public class DataScopeResolver : IDataScopeResolver
{
    private readonly IPermissionChecker _permissionChecker;
    private readonly IRoleRepo _roleRepo;
    private readonly ICacheService _cacheService;
    private readonly ILogger<DataScopeResolver> _logger;

    private const string UserDataScopesCacheKeyPrefix = "user:datascopes:";
    private static readonly TimeSpan DefaultCacheExpiration = TimeSpan.FromMinutes(5);

    public DataScopeResolver(
        IPermissionChecker permissionChecker,
        IRoleRepo roleRepo,
        ICacheService cacheService,
        ILogger<DataScopeResolver> logger)
    {
        _permissionChecker = permissionChecker;
        _roleRepo = roleRepo;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<DataScopeFilter> GetDataScopeAsync(long userId, CancellationToken cancellationToken = default)
    {
        var dataScopes = await GetUserDataScopesAsync(userId, cancellationToken);

        if (dataScopes.Count == 0)
        {
            // Default to Self scope if no data scopes are assigned
            return new DataScopeFilter
            {
                Type = DataScopeType.Self,
                UserId = userId
            };
        }

        // Find the highest priority scope
        // Priority order: All > Custom > Department/Region > Self
        var orderedScopes = dataScopes
            .Where(ds => ds.IsActive)
            .OrderByDescending(ds => ds.Priority)
            .ThenBy(ds => ds.Type) // Lower type value = higher priority (All = 1)
            .ToList();

        var primaryScope = orderedScopes.First();

        return new DataScopeFilter
        {
            Type = primaryScope.Type,
            UserId = userId,
            Condition = primaryScope.Condition
        };
    }

    public async Task<IReadOnlyList<DataScope>> GetUserDataScopesAsync(long userId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{UserDataScopesCacheKeyPrefix}{userId}";

        var cachedScopes = await _cacheService.GetAsync<List<DataScope>>(cacheKey, cancellationToken);
        if (cachedScopes != null)
            return cachedScopes;

        // Get user roles
        var roleIds = await _permissionChecker.GetUserRoleIdsAsync(userId, cancellationToken);
        if (roleIds.Count == 0)
            return [];

        // Get all data scopes from roles
        var allDataScopes = new List<DataScope>();

        foreach (var roleId in roleIds)
        {
            var role = await _roleRepo.GetWithDataScopesAsync(roleId, needTracking: false, cancellationToken: cancellationToken);
            if (role is not null)
            {
                var scopes = role.RoleDataScopes
                    .Select(rds => rds.DataScope)
                    .Where(ds => ds.IsActive)
                    .ToList();
                allDataScopes.AddRange(scopes);
            }
        }

        // Remove duplicates
        var uniqueScopes = allDataScopes
            .GroupBy(ds => ds.Id)
            .Select(g => g.First())
            .ToList();

        // Cache the result
        await _cacheService.SetAsync(cacheKey, uniqueScopes, DefaultCacheExpiration, cancellationToken);

        return uniqueScopes;
    }

    public async Task<bool> HasFullAccessAsync(long userId, CancellationToken cancellationToken = default)
    {
        var dataScope = await GetDataScopeAsync(userId, cancellationToken);
        return dataScope.HasFullAccess;
    }
}
