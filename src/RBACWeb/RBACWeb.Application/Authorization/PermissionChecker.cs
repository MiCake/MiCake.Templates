using Microsoft.EntityFrameworkCore;
using RBACWeb.Application.Cache;
using RBACWeb.Domain.Models.Authorization;
using RBACWeb.Domain.Repositories;

namespace RBACWeb.Application.Authorization;

/// <summary>
/// Implementation of permission checking with caching support.
/// </summary>
[InjectService(typeof(IPermissionChecker), Lifetime = MiCakeServiceLifetime.Scoped)]
public class PermissionChecker : IPermissionChecker
{
    private readonly IUserRepo _userRepo;
    private readonly IRoleRepo _roleRepo;
    private readonly ICacheService _cacheService;
    private readonly ILogger<PermissionChecker> _logger;

    private const string UserRolesCacheKeyPrefix = "user:roles:";
    private const string UserPermissionsCacheKeyPrefix = "user:permissions:";
    private const string UserPermissionsVersionKey = "user:permissions:version";
    private static readonly TimeSpan DefaultCacheExpiration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan PermissionVersionExpiration = TimeSpan.FromDays(1);

    public PermissionChecker(
        IUserRepo userRepo,
        IRoleRepo roleRepo,
        ICacheService cacheService,
        ILogger<PermissionChecker> logger)
    {
        _userRepo = userRepo;
        _roleRepo = roleRepo;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<bool> HasPermissionAsync(long userId, string permissionCode, CancellationToken cancellationToken = default)
    {
        var permissionCodes = await GetUserPermissionCodesAsync(userId, cancellationToken);
        return permissionCodes.Contains(permissionCode);
    }

    public async Task<IReadOnlyList<string>> GetUserPermissionCodesAsync(long userId, CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildUserPermissionsCacheKey(userId);

        var cachedCodes = await _cacheService.GetAsync<List<string>>(cacheKey, cancellationToken);
        if (cachedCodes != null)
            return cachedCodes;

        var permissions = await GetUserPermissionsAsync(userId, cancellationToken);
        var permissionCodes = permissions.Select(p => p.Code).Distinct().ToList();

        await _cacheService.SetAsync(cacheKey, permissionCodes, DefaultCacheExpiration, cancellationToken);
        return permissionCodes;
    }

    public async Task<IReadOnlyList<Permission>> GetUserPermissionsAsync(long userId, CancellationToken cancellationToken = default)
    {
        var roleIds = await GetUserRoleIdsAsync(userId, cancellationToken);
        if (roleIds.Count == 0)
            return [];

        var allPermissions = new List<Permission>();

        var roles = await _roleRepo.GetByIdsWithPermissionsAsync(roleIds, cancellationToken);
        foreach (var role in roles)
        {
            var permissions = role.RolePermissions
                .Where(rp => rp.IsGranted && rp.Permission.IsActive)
                .Select(rp => rp.Permission)
                .ToList();
            allPermissions.AddRange(permissions);
        }

        // Remove duplicates
        return allPermissions
            .GroupBy(p => p.Id)
            .Select(g => g.First())
            .ToList();
    }

    public async Task<IReadOnlyList<Role>> GetUserRolesAsync(long userId, CancellationToken cancellationToken = default)
    {
        var roleIds = await GetUserRoleIdsAsync(userId, cancellationToken);
        if (roleIds.Count == 0)
            return [];

        var roles = await _roleRepo.GetByIdsWithPermissionsAsync(roleIds, cancellationToken);
        return roles;
    }

    public async Task<IReadOnlyList<long>> GetUserRoleIdsAsync(long userId, CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildUserRolesCacheKey(userId);

        var cachedRoleIds = await _cacheService.GetAsync<List<long>>(cacheKey, cancellationToken);
        if (cachedRoleIds != null)
            return cachedRoleIds;

        // Load from database
        var user = await _userRepo.GetByIdWithIncludesAsync(
            userId,
            q => q.Include(u => u.UserRoles),
            needTracking: false,
            cancellationToken: cancellationToken);

        if (user is null)
        {
            _logger.LogWarning("User {UserId} not found when getting role IDs", userId);
            return [];
        }

        var roleIds = user.GetEffectiveRoleIds().ToList();

        // Cache the result
        await _cacheService.SetAsync(cacheKey, roleIds, DefaultCacheExpiration, cancellationToken);

        return roleIds;
    }

    public async Task InvalidateUserCacheAsync(long userId)
    {
        var roleCacheKey = BuildUserRolesCacheKey(userId);
        var permissionCacheKey = BuildUserPermissionsCacheKey(userId);
        await _cacheService.RemoveAsync(roleCacheKey);
        await _cacheService.RemoveAsync(permissionCacheKey);
        _logger.LogDebug("Invalidated user cache for user {UserId}", userId);
    }

    public async Task InvalidateRoleCacheAsync(long roleId)
    {
        var nextVersion = await IncrementUserPermissionsVersionAsync();
        _logger.LogDebug("Bumped user permission cache version to {Version} for role {RoleId}", nextVersion, roleId);
    }

    private string BuildUserRolesCacheKey(long userId)
    {
        return $"{UserRolesCacheKeyPrefix}{userId}";
    }

    private string BuildUserPermissionsCacheKey(long userId)
    {
        var version = GetUserPermissionsVersion();
        return $"{UserPermissionsCacheKeyPrefix}{userId}:{version}";
    }

    private int GetUserPermissionsVersion()
    {
        return _cacheService.Get<int?>(UserPermissionsVersionKey) ?? 1;
    }

    private async Task<int> IncrementUserPermissionsVersionAsync(CancellationToken cancellationToken = default)
    {
        var current = await _cacheService.GetAsync<int?>(UserPermissionsVersionKey, cancellationToken) ?? 1;
        var next = current + 1;
        await _cacheService.SetAsync(UserPermissionsVersionKey, next, PermissionVersionExpiration, cancellationToken);
        return next;
    }
}
