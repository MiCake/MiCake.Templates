using AutoMapper;
using RBACWeb.Application.ErrorCodes;
using RBACWeb.Contracts.Dtos.Authorization;
using RBACWeb.Domain.Models.Authorization;
using RBACWeb.Domain.Repositories;
using DomainPermissionAction = RBACWeb.Domain.Enums.Authorization.PermissionAction;

namespace RBACWeb.Application.Services.Authorization;

/// <summary>
/// Service for managing permissions.
/// </summary>
[InjectService(Lifetime = MiCakeServiceLifetime.Scoped)]
public class PermissionService
{
    private readonly IPermissionRepo _permissionRepo;
    private readonly IResourceRepo _resourceRepo;
    private readonly IMapper _mapper;
    private readonly ILogger<PermissionService> _logger;

    public PermissionService(
        IPermissionRepo permissionRepo,
        IResourceRepo resourceRepo,
        IMapper mapper,
        ILogger<PermissionService> logger)
    {
        _permissionRepo = permissionRepo;
        _resourceRepo = resourceRepo;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Gets all active permissions.
    /// </summary>
    public async Task<OperationResult<IReadOnlyList<PermissionDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var permissions = await _permissionRepo.GetActivePermissionsAsync(cancellationToken);
        var dtos = _mapper.Map<IReadOnlyList<PermissionDto>>(permissions);
        return OperationResult<IReadOnlyList<PermissionDto>>.Success(dtos);
    }

    /// <summary>
    /// Gets a permission by ID.
    /// </summary>
    public async Task<OperationResult<PermissionDto?>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var permission = await _permissionRepo.FindAsync(id, cancellationToken);
        if (permission is null)
            return OperationResult<PermissionDto?>.Failure("Permission not found", AuthorizationErrorCodes.PermissionNotFound);

        var dto = _mapper.Map<PermissionDto>(permission);
        return OperationResult<PermissionDto?>.Success(dto);
    }

    /// <summary>
    /// Gets permissions by resource ID.
    /// </summary>
    public async Task<OperationResult<IReadOnlyList<PermissionDto>>> GetByResourceAsync(long resourceId, CancellationToken cancellationToken = default)
    {
        var permissions = await _permissionRepo.GetByResourceIdAsync(resourceId, cancellationToken);
        var dtos = _mapper.Map<IReadOnlyList<PermissionDto>>(permissions);
        return OperationResult<IReadOnlyList<PermissionDto>>.Success(dtos);
    }

    /// <summary>
    /// Creates a new permission.
    /// </summary>
    public async Task<OperationResult<PermissionDto?>> CreateAsync(CreatePermissionDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating permission with code: {Code}", dto.Code);

        // Check for duplicate code
        if (await _permissionRepo.ExistsByCodeAsync(dto.Code, cancellationToken))
            return OperationResult<PermissionDto?>.Failure("Permission with this code already exists", AuthorizationErrorCodes.PermissionAlreadyExists);

        // Validate resource exists when provided
        if (dto.ResourceId.HasValue)
        {
            var resource = await _resourceRepo.FindAsync(dto.ResourceId.Value, cancellationToken);
            if (resource is null)
                return OperationResult<PermissionDto?>.Failure("Resource not found", AuthorizationErrorCodes.ResourceNotFound);
        }

        var permission = Permission.Create(dto.Code, dto.Name, dto.ResourceId, (DomainPermissionAction)dto.Action, dto.Description);

        await _permissionRepo.AddAsync(permission, cancellationToken);
        await _permissionRepo.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Permission {PermissionId} created successfully", permission.Id);
        return OperationResult<PermissionDto?>.Success(_mapper.Map<PermissionDto>(permission));
    }

    /// <summary>
    /// Updates a permission.
    /// </summary>
    public async Task<OperationResult<PermissionDto?>> UpdateAsync(long id, UpdatePermissionDto dto, CancellationToken cancellationToken = default)
    {
        var permission = await _permissionRepo.FindAsync(id, cancellationToken);
        if (permission is null)
            return OperationResult<PermissionDto?>.Failure("Permission not found", AuthorizationErrorCodes.PermissionNotFound);

        permission.Update(dto.Name, dto.Description);
        await _permissionRepo.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Permission {PermissionId} updated successfully", id);
        return OperationResult<PermissionDto?>.Success(_mapper.Map<PermissionDto>(permission));
    }

    /// <summary>
    /// Deactivates a permission.
    /// </summary>
    public async Task<OperationResult> DeactivateAsync(long id, CancellationToken cancellationToken = default)
    {
        var permission = await _permissionRepo.FindAsync(id, cancellationToken);
        if (permission is null)
            return OperationResult.Failure("Permission not found", AuthorizationErrorCodes.PermissionNotFound);

        permission.Deactivate();
        await _permissionRepo.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Permission {PermissionId} deactivated successfully", id);
        return OperationResult.Success();
    }
}
