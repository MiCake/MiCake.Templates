using RBACWeb.Domain.Models.Authorization;

namespace RBACWeb.Application.Authorization;

/// <summary>
/// Service for checking user permissions.
/// </summary>
public interface IPermissionChecker
{
    /// <summary>
    /// Checks if the user has a specific permission.
    /// </summary>
    Task<bool> HasPermissionAsync(long userId, string permissionCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all permission codes for a user.
    /// </summary>
    Task<IReadOnlyList<string>> GetUserPermissionCodesAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all permissions for a user.
    /// </summary>
    Task<IReadOnlyList<Permission>> GetUserPermissionsAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all roles for a user.
    /// </summary>
    Task<IReadOnlyList<Role>> GetUserRolesAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the user has a specific role.
    /// </summary>
    Task<bool> IsInRoleAsync(long userId, string roleCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all role IDs for a user.
    /// </summary>
    Task<IReadOnlyList<long>> GetUserRoleIdsAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates the permission cache for a user.
    /// </summary>
    Task InvalidateUserCacheAsync(long userId);

    /// <summary>
    /// Invalidates the permission cache for a role.
    /// </summary>
    Task InvalidateRoleCacheAsync(long roleId);
}
