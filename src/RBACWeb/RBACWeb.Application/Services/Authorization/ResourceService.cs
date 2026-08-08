using AutoMapper;
using RBACWeb.Application.ErrorCodes;
using RBACWeb.Contracts.Dtos.Authorization;
using RBACWeb.Domain.Models.Authorization;
using RBACWeb.Domain.Repositories;
using DomainResourceType = RBACWeb.Domain.Enums.Authorization.ResourceType;

namespace RBACWeb.Application.Services.Authorization;

/// <summary>
/// Service for managing resources.
/// </summary>
[InjectService(Lifetime = MiCakeServiceLifetime.Scoped)]
public class ResourceService
{
    private readonly IResourceRepo _resourceRepo;
    private readonly IMapper _mapper;
    private readonly ILogger<ResourceService> _logger;

    public ResourceService(
        IResourceRepo resourceRepo,
        IMapper mapper,
        ILogger<ResourceService> logger)
    {
        _resourceRepo = resourceRepo;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Gets all active resources.
    /// </summary>
    public async Task<OperationResult<IReadOnlyList<ResourceDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var resources = await _resourceRepo.GetActiveResourcesAsync(cancellationToken);
        var dtos = _mapper.Map<IReadOnlyList<ResourceDto>>(resources);
        return OperationResult<IReadOnlyList<ResourceDto>>.Success(dtos);
    }

    /// <summary>
    /// Gets resources as a tree structure.
    /// </summary>
    public async Task<OperationResult<IReadOnlyList<ResourceTreeDto>>> GetTreeAsync(CancellationToken cancellationToken = default)
    {
        var allResources = await _resourceRepo.GetActiveResourcesAsync(cancellationToken);
        var resourceDict = allResources.ToDictionary(r => r.Id);

        // Build tree
        var rootResources = allResources
            .Where(r => r.ParentId == null)
            .Select(r => BuildResourceTree(r, resourceDict))
            .OrderBy(r => r.SortOrder)
            .ToList();

        return OperationResult<IReadOnlyList<ResourceTreeDto>>.Success(rootResources);
    }

    /// <summary>
    /// Gets a resource by ID.
    /// </summary>
    public async Task<OperationResult<ResourceDto?>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var resource = await _resourceRepo.FindAsync(id, cancellationToken);
        if (resource is null)
            return OperationResult<ResourceDto?>.Failure("Resource not found", AuthorizationErrorCodes.ResourceNotFound);

        var dto = _mapper.Map<ResourceDto>(resource);
        return OperationResult<ResourceDto?>.Success(dto);
    }

    /// <summary>
    /// Creates a new resource.
    /// </summary>
    public async Task<OperationResult<ResourceDto?>> CreateAsync(CreateResourceDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating resource with code: {Code}", dto.Code);

        // Check for duplicate code
        if (await _resourceRepo.ExistsByCodeAsync(dto.Code, cancellationToken))
            return OperationResult<ResourceDto?>.Failure("Resource with this code already exists", AuthorizationErrorCodes.ResourceAlreadyExists);

        // Validate parent exists if specified
        if (dto.ParentId.HasValue)
        {
            var parent = await _resourceRepo.FindAsync(dto.ParentId.Value, cancellationToken);
            if (parent is null)
                return OperationResult<ResourceDto?>.Failure("Parent resource not found", AuthorizationErrorCodes.ResourceNotFound);
        }

        var resource = Resource.Create(
            dto.Code,
            dto.Name,
            (DomainResourceType)dto.Type,
            dto.Description,
            dto.Path,
            dto.ParentId,
            dto.SortOrder);

        await _resourceRepo.AddAndGetIdAsync(resource, cancellationToken);

        _logger.LogInformation("Resource {ResourceId} created successfully", resource.Id);
        return OperationResult<ResourceDto?>.Success(_mapper.Map<ResourceDto>(resource));
    }

    /// <summary>
    /// Updates a resource.
    /// </summary>
    public async Task<OperationResult<ResourceDto?>> UpdateAsync(long id, UpdateResourceDto dto, CancellationToken cancellationToken = default)
    {
        var resource = await _resourceRepo.FindAsync(id, cancellationToken);
        if (resource is null)
            return OperationResult<ResourceDto?>.Failure("Resource not found", AuthorizationErrorCodes.ResourceNotFound);

        resource.Update(dto.Name, dto.Description, dto.Path, dto.SortOrder);

        _logger.LogInformation("Resource {ResourceId} updated successfully", id);
        return OperationResult<ResourceDto?>.Success(_mapper.Map<ResourceDto>(resource));
    }

    /// <summary>
    /// Deactivates a resource.
    /// </summary>
    public async Task<OperationResult> DeactivateAsync(long id, CancellationToken cancellationToken = default)
    {
        var resource = await _resourceRepo.FindAsync(id, cancellationToken);
        if (resource is null)
            return OperationResult.Failure("Resource not found", AuthorizationErrorCodes.ResourceNotFound);

        resource.Deactivate();

        _logger.LogInformation("Resource {ResourceId} deactivated successfully", id);
        return OperationResult.Success();
    }

    private ResourceTreeDto BuildResourceTree(Resource resource, Dictionary<long, Resource> resourceDict)
    {
        var dto = _mapper.Map<ResourceTreeDto>(resource);
        dto.Children = resourceDict.Values
            .Where(r => r.ParentId == resource.Id)
            .OrderBy(r => r.SortOrder)
            .Select(r => BuildResourceTree(r, resourceDict))
            .ToList();
        return dto;
    }
}
