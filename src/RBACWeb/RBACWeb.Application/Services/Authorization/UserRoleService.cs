using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RBACWeb.Application.Authorization;
using RBACWeb.Application.ErrorCodes;
using RBACWeb.Contracts.Dtos.Authorization;
using RBACWeb.Domain.Repositories;

namespace RBACWeb.Application.Services.Authorization;

/// <summary>
/// Service for managing user role assignments.
/// </summary>
[InjectService(Lifetime = MiCakeServiceLifetime.Scoped)]
public class UserRoleService
{
    private readonly IUserRepo _userRepo;
    private readonly IRoleRepo _roleRepo;
    private readonly IPermissionChecker _permissionChecker;
    private readonly IMapper _mapper;
    private readonly ILogger<UserRoleService> _logger;

    public UserRoleService(
        IUserRepo userRepo,
        IRoleRepo roleRepo,
        IPermissionChecker permissionChecker,
        IMapper mapper,
        ILogger<UserRoleService> logger)
    {
        _userRepo = userRepo;
        _roleRepo = roleRepo;
        _permissionChecker = permissionChecker;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Gets all roles assigned to a user.
    /// </summary>
    public async Task<OperationResult<IReadOnlyList<UserRoleDto>>> GetUserRolesAsync(long userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepo.GetByIdWithIncludesAsync(
            userId,
            q => q.Include(u => u.UserRoles).ThenInclude(ur => ur.Role),
            needTracking: false,
            cancellationToken: cancellationToken);

        if (user is null)
            return OperationResult<IReadOnlyList<UserRoleDto>>.Failure("User not found", AuthErrorCodes.UserNotFound);

        var dtos = _mapper.Map<IReadOnlyList<UserRoleDto>>(user.UserRoles.Where(ur => ur.IsEffective()));
        return OperationResult<IReadOnlyList<UserRoleDto>>.Success(dtos);
    }

    /// <summary>
    /// Gets all permissions for a user.
    /// </summary>
    public async Task<OperationResult<UserPermissionsDto>> GetUserPermissionsAsync(long userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepo.FindAsync(userId, cancellationToken);
        if (user is null)
            return OperationResult<UserPermissionsDto>.Failure("User not found", AuthErrorCodes.UserNotFound);

        var permissionCodes = await _permissionChecker.GetUserPermissionCodesAsync(userId, cancellationToken);

        var dto = new UserPermissionsDto
        {
            UserId = userId,
            PermissionCodes = permissionCodes.ToList()
        };

        return OperationResult<UserPermissionsDto>.Success(dto);
    }

    /// <summary>
    /// Assigns a role to a user.
    /// </summary>
    public async Task<OperationResult> AssignRoleAsync(long userId, AssignUserRoleDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Assigning role {RoleId} to user {UserId}", dto.RoleId, userId);

        var user = await _userRepo.GetByIdWithIncludesAsync(
            userId,
            q => q.Include(u => u.UserRoles),
            needTracking: true,
            cancellationToken: cancellationToken);

        if (user is null)
            return OperationResult.Failure("User not found", AuthErrorCodes.UserNotFound);

        // Validate role exists
        var role = await _roleRepo.FindAsync(dto.RoleId, cancellationToken);
        if (role is null)
            return OperationResult.Failure("Role not found", AuthorizationErrorCodes.RoleNotFound);

        // Check if already assigned
        if (user.HasRole(dto.RoleId))
            return OperationResult.Failure("Role already assigned to user", AuthorizationErrorCodes.UserRoleAlreadyAssigned);

        user.AssignRole(dto.RoleId, dto.ExpiresAt);
        await _userRepo.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await _permissionChecker.InvalidateUserCacheAsync(userId);

        _logger.LogInformation("Role {RoleId} assigned to user {UserId} successfully", dto.RoleId, userId);
        return OperationResult.Success();
    }

    /// <summary>
    /// Removes a role from a user.
    /// </summary>
    public async Task<OperationResult> RemoveRoleAsync(long userId, long roleId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Removing role {RoleId} from user {UserId}", roleId, userId);

        var user = await _userRepo.GetByIdWithIncludesAsync(
            userId,
            q => q.Include(u => u.UserRoles),
            needTracking: true,
            cancellationToken: cancellationToken);

        if (user is null)
            return OperationResult.Failure("User not found", AuthErrorCodes.UserNotFound);

        if (!user.HasRole(roleId))
            return OperationResult.Failure("Role not assigned to user", AuthorizationErrorCodes.UserRoleNotFound);

        user.RemoveRole(roleId);
        await _userRepo.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await _permissionChecker.InvalidateUserCacheAsync(userId);

        _logger.LogInformation("Role {RoleId} removed from user {UserId} successfully", roleId, userId);
        return OperationResult.Success();
    }
}
