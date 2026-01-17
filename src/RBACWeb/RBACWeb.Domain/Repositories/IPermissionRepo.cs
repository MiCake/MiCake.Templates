using RBACWeb.Domain.Models.Authorization;

namespace RBACWeb.Domain.Repositories;

/// <summary>
/// Repository interface for Permission aggregate.
/// </summary>
public interface IPermissionRepo : IRepositoryHasPagingQuery<Permission, long>
{
    /// <summary>
    /// Gets a permission by its unique code.
    /// </summary>
    Task<Permission?> GetByCodeAsync(string code, bool needTracking = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets permissions by resource ID.
    /// </summary>
    Task<IReadOnlyList<Permission>> GetByResourceIdAsync(long resourceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active permissions.
    /// </summary>
    Task<IReadOnlyList<Permission>> GetActivePermissionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets permissions by multiple IDs.
    /// </summary>
    Task<IReadOnlyList<Permission>> GetByIdsAsync(IEnumerable<long> ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets permissions by multiple codes.
    /// </summary>
    Task<IReadOnlyList<Permission>> GetByCodesAsync(IEnumerable<string> codes, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a permission code already exists.
    /// </summary>
    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);
}
