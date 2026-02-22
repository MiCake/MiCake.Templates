using AutoMapper;
using RBACWeb.Application.Authorization;
using RBACWeb.Application.ErrorCodes;
using RBACWeb.Contracts.Dtos.Authorization;
using RBACWeb.Domain.Models.Authorization;
using RBACWeb.Domain.Repositories;

namespace RBACWeb.Application.Services.Authorization;

/// <summary>
/// Service for managing roles.
/// </summary>
[InjectService(Lifetime = MiCakeServiceLifetime.Scoped)]
public class RoleService
{
    private readonly IRoleRepo _roleRepo;
    private readonly IPermissionRepo _permissionRepo;
    private readonly IDataScopeRepo _dataScopeRepo;
    private readonly IPermissionChecker _permissionChecker;
    private readonly IMapper _mapper;
    private readonly ILogger<RoleService> _logger;

    public RoleService(
        IRoleRepo roleRepo,
        IPermissionRepo permissionRepo,
        IDataScopeRepo dataScopeRepo,
        IPermissionChecker permissionChecker,
        IMapper mapper,
        ILogger<RoleService> logger)
    {
        _roleRepo = roleRepo;
        _permissionRepo = permissionRepo;
        _dataScopeRepo = dataScopeRepo;
        _permissionChecker = permissionChecker;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Gets all roles.
    /// </summary>
    public async Task<OperationResult<IReadOnlyList<RoleDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _roleRepo.GetActiveRolesAsync(cancellationToken);
        var dtos = _mapper.Map<IReadOnlyList<RoleDto>>(roles);
        return OperationResult<IReadOnlyList<RoleDto>>.Success(dtos);
    }

    /// <summary>
    /// Gets a role by ID with details.
    /// </summary>
    public async Task<OperationResult<RoleDetailDto?>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var role = await _roleRepo.GetWithAllIncludesAsync(id, needTracking: false, cancellationToken: cancellationToken);
        if (role is null)
            return OperationResult<RoleDetailDto?>.Failure("Role not found", AuthorizationErrorCodes.RoleNotFound);

        var dto = _mapper.Map<RoleDetailDto>(role);
        return OperationResult<RoleDetailDto?>.Success(dto);
    }

    /// <summary>
    /// Creates a new role.
    /// </summary>
    public async Task<OperationResult<RoleDto?>> CreateAsync(CreateRoleDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating role: {Name}", dto.Name);

        var role = Role.Create(dto.Name, dto.Description);

        await _roleRepo.AddAsync(role, cancellationToken);
        await _roleRepo.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Role {RoleId} created successfully", role.Id);
        return OperationResult<RoleDto?>.Success(_mapper.Map<RoleDto>(role));
    }

    /// <summary>
    /// Updates a role.
    /// </summary>
    public async Task<OperationResult<RoleDto?>> UpdateAsync(long id, UpdateRoleDto dto, CancellationToken cancellationToken = default)
    {
        var role = await _roleRepo.FindAsync(id, cancellationToken);
        if (role is null)
            return OperationResult<RoleDto?>.Failure("Role not found", AuthorizationErrorCodes.RoleNotFound);

        role.Update(dto.Name, dto.Description);
        await _roleRepo.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await _permissionChecker.InvalidateRoleCacheAsync(id);

        _logger.LogInformation("Role {RoleId} updated successfully", id);
        return OperationResult<RoleDto?>.Success(_mapper.Map<RoleDto>(role));
    }

    /// <summary>
    /// Deletes a role.
    /// </summary>
    public async Task<OperationResult> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var role = await _roleRepo.FindAsync(id, cancellationToken);
        if (role is null)
            return OperationResult.Failure("Role not found", AuthorizationErrorCodes.RoleNotFound);

        if (role.IsSystem)
            return OperationResult.Failure("Cannot delete system role", AuthorizationErrorCodes.RoleIsSystem);

        role.Deactivate();
        await _roleRepo.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await _permissionChecker.InvalidateRoleCacheAsync(id);

        _logger.LogInformation("Role {RoleId} deleted successfully", id);
        return OperationResult.Success();
    }

    /// <summary>
    /// Assigns permissions to a role.
    /// </summary>
    public async Task<OperationResult> AssignPermissionsAsync(long roleId, AssignPermissionsDto dto, CancellationToken cancellationToken = default)
    {
        var role = await _roleRepo.GetWithPermissionsAsync(roleId, needTracking: true, cancellationToken: cancellationToken);
        if (role is null)
            return OperationResult.Failure("Role not found", AuthorizationErrorCodes.RoleNotFound);

        // Validate permissions exist
        var permissions = await _permissionRepo.GetByIdsAsync(dto.PermissionIds, cancellationToken);
        var existingIds = permissions.Select(p => p.Id).ToHashSet();
        var missingIds = dto.PermissionIds.Where(id => !existingIds.Contains(id)).ToList();

        if (missingIds.Any())
            return OperationResult.Failure($"Permissions not found: {string.Join(", ", missingIds)}", AuthorizationErrorCodes.PermissionNotFound);

        // Clear existing permissions and add new ones
        var currentPermissionIds = role.RolePermissions.Select(rp => rp.PermissionId).ToList();

        // Remove permissions not in the new list
        foreach (var permissionId in currentPermissionIds.Where(id => !dto.PermissionIds.Contains(id)))
        {
            role.RemovePermission(permissionId);
        }

        // Add new permissions
        foreach (var permissionId in dto.PermissionIds.Where(id => !currentPermissionIds.Contains(id)))
        {
            role.AddPermission(permissionId);
        }

        await _roleRepo.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await _permissionChecker.InvalidateRoleCacheAsync(roleId);

        _logger.LogInformation("Permissions assigned to role {RoleId}", roleId);
        return OperationResult.Success();
    }

    /// <summary>
    /// Assigns data scopes to a role.
    /// </summary>
    public async Task<OperationResult> AssignDataScopesAsync(long roleId, AssignDataScopesDto dto, CancellationToken cancellationToken = default)
    {
        var role = await _roleRepo.GetWithDataScopesAsync(roleId, needTracking: true, cancellationToken: cancellationToken);
        if (role is null)
            return OperationResult.Failure("Role not found", AuthorizationErrorCodes.RoleNotFound);

        // Validate data scopes exist
        var dataScopes = await _dataScopeRepo.GetByIdsAsync(dto.DataScopeIds, cancellationToken);
        var existingIds = dataScopes.Select(ds => ds.Id).ToHashSet();
        var missingIds = dto.DataScopeIds.Where(id => !existingIds.Contains(id)).ToList();

        if (missingIds.Any())
            return OperationResult.Failure($"Data scopes not found: {string.Join(", ", missingIds)}", AuthorizationErrorCodes.DataScopeNotFound);

        // Clear existing data scopes and add new ones
        var currentDataScopeIds = role.RoleDataScopes.Select(rds => rds.DataScopeId).ToList();

        // Remove data scopes not in the new list
        foreach (var dataScopeId in currentDataScopeIds.Where(id => !dto.DataScopeIds.Contains(id)))
        {
            role.RemoveDataScope(dataScopeId);
        }

        // Add new data scopes
        foreach (var dataScopeId in dto.DataScopeIds.Where(id => !currentDataScopeIds.Contains(id)))
        {
            role.AddDataScope(dataScopeId);
        }

        await _roleRepo.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Data scopes assigned to role {RoleId}", roleId);
        return OperationResult.Success();
    }
}
