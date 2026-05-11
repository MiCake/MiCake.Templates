using RBACWeb.Domain.Enums.Authorization;
using RBACWeb.Domain.Models.Authorization;

namespace RBACWeb.Domain.Repositories;

/// <summary>
/// Repository interface for DataScope aggregate.
/// </summary>
public interface IDataScopeRepo : IRepositoryHasPagingQuery<DataScope, long>
{
    /// <summary>
    /// Gets a data scope by its unique code.
    /// </summary>
    Task<DataScope?> GetByCodeAsync(string code, bool needTracking = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets data scopes by type.
    /// </summary>
    Task<IReadOnlyList<DataScope>> GetByTypeAsync(DataScopeType type, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active data scopes.
    /// </summary>
    Task<IReadOnlyList<DataScope>> GetActiveDataScopesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets data scopes by multiple IDs.
    /// </summary>
    Task<IReadOnlyList<DataScope>> GetByIdsAsync(IEnumerable<long> ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a data scope code already exists.
    /// </summary>
    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);
}
