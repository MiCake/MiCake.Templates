using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using MiCake.DDD.Uow;
using RBACWeb.Application.Audit;
using RBACWeb.Application.Authorization;

namespace RBACWeb.Web.Authorization;

/// <summary>
/// Handler for permission-based authorization.
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ICurrentUser _currentUser;
    private readonly IStandaloneUnitOfWorkExecutor _standaloneUnitOfWorkExecutor;
    private readonly ILogger<PermissionAuthorizationHandler> _logger;

    public PermissionAuthorizationHandler(
        ICurrentUser currentUser,
        IStandaloneUnitOfWorkExecutor standaloneUnitOfWorkExecutor,
        ILogger<PermissionAuthorizationHandler> logger)
    {
        _currentUser = currentUser;
        _standaloneUnitOfWorkExecutor = standaloneUnitOfWorkExecutor;
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

        // The authorization stage runs before MiCake's action-filter unit of work is
        // created, so the permission query executes inside an explicit standalone UoW
        // boundary instead of relying on an ambient one.
        var hasPermission = await _standaloneUnitOfWorkExecutor.ExecuteAsync(
            async (provider, cancellationToken) =>
            {
                var permissionChecker = provider.GetRequiredService<IPermissionChecker>();
                return await permissionChecker.HasPermissionAsync(userId.Value, requirement.PermissionCode, cancellationToken);
            },
            UnitOfWorkOptions.ReadOnly);

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

