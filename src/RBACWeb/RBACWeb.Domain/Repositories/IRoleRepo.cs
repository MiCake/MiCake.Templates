using RBACWeb.Domain.Models.Authorization;

namespace RBACWeb.Domain.Repositories;

/// <summary>
/// Repository interface for Role aggregate.
/// </summary>
public interface IRoleRepo : IRepositoryHasPagingQuery<Role, long>
{
    /// <summary>
    /// Gets a role with its permissions included.
    /// </summary>
    Task<Role?> GetWithPermissionsAsync(long id, bool needTracking = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a role with its data scopes included.
    /// </summary>
    Task<Role?> GetWithDataScopesAsync(long id, bool needTracking = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a role with all related entities included.
    /// </summary>
    Task<Role?> GetWithAllIncludesAsync(long id, bool needTracking = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active roles.
    /// </summary>
    Task<IReadOnlyList<Role>> GetActiveRolesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets roles by multiple IDs with permissions included.
    /// </summary>
    Task<IReadOnlyList<Role>> GetByIdsWithPermissionsAsync(IEnumerable<long> roleIds, CancellationToken cancellationToken = default);

}
