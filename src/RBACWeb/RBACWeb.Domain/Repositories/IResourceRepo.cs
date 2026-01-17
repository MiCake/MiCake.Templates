using RBACWeb.Domain.Enums.Authorization;
using RBACWeb.Domain.Models.Authorization;

namespace RBACWeb.Domain.Repositories;

/// <summary>
/// Repository interface for Resource aggregate.
/// </summary>
public interface IResourceRepo : IRepositoryHasPagingQuery<Resource, long>
{
    /// <summary>
    /// Gets a resource by its unique code.
    /// </summary>
    Task<Resource?> GetByCodeAsync(string code, bool needTracking = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets resources by type.
    /// </summary>
    Task<IReadOnlyList<Resource>> GetByTypeAsync(ResourceType type, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets child resources of a parent.
    /// </summary>
    Task<IReadOnlyList<Resource>> GetChildrenAsync(long parentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all root resources (no parent).
    /// </summary>
    Task<IReadOnlyList<Resource>> GetRootResourcesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active resources.
    /// </summary>
    Task<IReadOnlyList<Resource>> GetActiveResourcesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a resource with its permissions included.
    /// </summary>
    Task<Resource?> GetWithPermissionsAsync(long id, bool needTracking = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a resource code already exists.
    /// </summary>
    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);
}
