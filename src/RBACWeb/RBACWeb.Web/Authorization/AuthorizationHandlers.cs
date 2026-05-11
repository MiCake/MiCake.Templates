using Microsoft.AspNetCore.Authorization;
using RBACWeb.Application.Audit;

namespace RBACWeb.Web.Authorization;

/// <summary>
/// Handler for permission-based authorization.
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<PermissionAuthorizationHandler> _logger;

    public PermissionAuthorizationHandler(
        ICurrentUser currentUser,
        ILogger<PermissionAuthorizationHandler> logger)
    {
        _currentUser = currentUser;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userId = _currentUser.GetCurrentUserId();
        if (userId == null)
        {
            _logger.LogDebug("Authorization failed: User not authenticated");
            return;
        }

        var hasPermission = await _currentUser.HasPermissionAsync(requirement.PermissionCode);
        if (hasPermission)
        {
            _logger.LogDebug("User {UserId} has permission {Permission}", userId, requirement.PermissionCode);
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogDebug("User {UserId} does not have permission {Permission}", userId, requirement.PermissionCode);
        }
    }
}

