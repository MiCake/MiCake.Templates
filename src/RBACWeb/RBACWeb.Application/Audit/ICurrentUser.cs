using Microsoft.AspNetCore.Http;
using RBACWeb.Application.Authorization;
using RBACWeb.Common.Auth;

namespace RBACWeb.Application.Audit;

/// <summary>
/// A service to get information about the current user from JWT claims.
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// Gets the current user's ID from JWT claims.
    /// </summary>
    long? GetCurrentUserId();

    /// <summary>
    /// Gets the current user's role IDs from JWT claims.
    /// </summary>
    IEnumerable<long> GetRoleIds();

    /// <summary>
    /// Gets the current user's roles (from cache).
    /// </summary>
    Task<IEnumerable<string>> GetRolesAsync();

    /// <summary>
    /// Gets the current user's permission codes (from cache).
    /// </summary>
    Task<IEnumerable<string>> GetPermissionsAsync();

    /// <summary>
    /// Checks if the current user has a specific permission (from cache).
    /// </summary>
    Task<bool> HasPermissionAsync(string permissionCode);

    /// <summary>
    /// Checks if the current user has a specific role (from cache).
    /// </summary>
    Task<bool> IsInRoleAsync(string roleCode);

    /// <summary>
    /// Gets the current user's data scope filter (from cache).
    /// </summary>
    Task<DataScopeFilter?> GetDataScopeAsync();
}

[InjectService(typeof(ICurrentUser), Lifetime = MiCakeServiceLifetime.Scoped)]
internal class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPermissionChecker _permissionChecker;
    private readonly IDataScopeResolver _dataScopeResolver;

    public CurrentUser(
        IHttpContextAccessor httpContextAccessor,
        IPermissionChecker permissionChecker,
        IDataScopeResolver dataScopeResolver)
    {
        _httpContextAccessor = httpContextAccessor;
        _permissionChecker = permissionChecker;
        _dataScopeResolver = dataScopeResolver;
    }

    public long? GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(JwtClaimTypes.UserId);
        if (userIdClaim != null && long.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }
        return null;
    }

    public IEnumerable<long> GetRoleIds()
    {
        // Get all role claims (each role ID is a separate claim)
        var roleClaims = _httpContextAccessor.HttpContext?.User?.FindAll(JwtClaimTypes.Roles);
        if (roleClaims == null)
            return [];

        return roleClaims
            .Select(c => long.TryParse(c.Value, out var id) ? id : 0)
            .Where(id => id > 0);
    }

    public async Task<IEnumerable<string>> GetRolesAsync()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return [];

        var roles = await _permissionChecker.GetUserRolesAsync(userId.Value);
        return roles.Select(r => r.Code);
    }

    public async Task<IEnumerable<string>> GetPermissionsAsync()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return [];

        return await _permissionChecker.GetUserPermissionCodesAsync(userId.Value);
    }

    public async Task<bool> HasPermissionAsync(string permissionCode)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return false;

        return await _permissionChecker.HasPermissionAsync(userId.Value, permissionCode);
    }

    public async Task<bool> IsInRoleAsync(string roleCode)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return false;

        return await _permissionChecker.IsInRoleAsync(userId.Value, roleCode);
    }

    public async Task<DataScopeFilter?> GetDataScopeAsync()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return null;

        return await _dataScopeResolver.GetDataScopeAsync(userId.Value);
    }
}