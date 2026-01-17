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
    private const string RolePermissionsCacheKeyPrefix = "role:permissions:";
    private static readonly TimeSpan DefaultCacheExpiration = TimeSpan.FromMinutes(5);

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
        var permissions = await GetUserPermissionsAsync(userId, cancellationToken);
        return permissions.Select(p => p.Code).ToList();
    }

    public async Task<IReadOnlyList<Permission>> GetUserPermissionsAsync(long userId, CancellationToken cancellationToken = default)
    {
        var roleIds = await GetUserRoleIdsAsync(userId, cancellationToken);
        if (roleIds.Count == 0)
            return [];

        var allPermissions = new List<Permission>();

        foreach (var roleId in roleIds)
        {
            var permissions = await GetRolePermissionsFromCacheAsync(roleId, cancellationToken);
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

    public async Task<bool> IsInRoleAsync(long userId, string roleCode, CancellationToken cancellationToken = default)
    {
        var roles = await GetUserRolesAsync(userId, cancellationToken);
        return roles.Any(r => r.Code == roleCode);
    }

    public async Task<IReadOnlyList<long>> GetUserRoleIdsAsync(long userId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{UserRolesCacheKeyPrefix}{userId}";

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
        var cacheKey = $"{UserRolesCacheKeyPrefix}{userId}";
        await _cacheService.RemoveAsync(cacheKey);
        _logger.LogDebug("Invalidated user cache for user {UserId}", userId);
    }

    public async Task InvalidateRoleCacheAsync(long roleId)
    {
        var cacheKey = $"{RolePermissionsCacheKeyPrefix}{roleId}";
        await _cacheService.RemoveAsync(cacheKey);
        _logger.LogDebug("Invalidated role cache for role {RoleId}", roleId);
    }

    private async Task<IReadOnlyList<Permission>> GetRolePermissionsFromCacheAsync(long roleId, CancellationToken cancellationToken)
    {
        var cacheKey = $"{RolePermissionsCacheKeyPrefix}{roleId}";

        var cachedPermissions = await _cacheService.GetAsync<List<Permission>>(cacheKey, cancellationToken);
        if (cachedPermissions != null)
            return cachedPermissions;

        // Load from database
        var role = await _roleRepo.GetWithPermissionsAsync(roleId, needTracking: false, cancellationToken: cancellationToken);
        if (role is null)
        {
            _logger.LogWarning("Role {RoleId} not found when getting permissions", roleId);
            return [];
        }

        var permissions = role.RolePermissions
            .Where(rp => rp.IsGranted && rp.Permission.IsActive)
            .Select(rp => rp.Permission)
            .ToList();

        // Cache the result
        await _cacheService.SetAsync(cacheKey, permissions, DefaultCacheExpiration, cancellationToken);

        return permissions;
    }
}
