using RBACWeb.Domain.Enums.Authorization;
using RBACWeb.Domain.Models.Authorization;

namespace RBACWeb.Application.Authorization;

/// <summary>
/// Represents a resolved data scope filter.
/// </summary>
public class DataScopeFilter
{
    /// <summary>
    /// The type of data scope.
    /// </summary>
    public DataScopeType Type { get; set; }

    /// <summary>
    /// The user ID for Self scope.
    /// </summary>
    public long? UserId { get; set; }

    /// <summary>
    /// Custom filter condition for Custom scope.
    /// </summary>
    public string? Condition { get; set; }

    /// <summary>
    /// Whether the user has full access (All scope).
    /// </summary>
    public bool HasFullAccess => Type == DataScopeType.All;

    /// <summary>
    /// Whether the scope restricts to self only.
    /// </summary>
    public bool IsSelfOnly => Type == DataScopeType.Self;
}

/// <summary>
/// Service for resolving data scope filters.
/// </summary>
public interface IDataScopeResolver
{
    /// <summary>
    /// Gets the effective data scope filter for a user.
    /// </summary>
    Task<DataScopeFilter> GetDataScopeAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all data scopes assigned to a user through their roles.
    /// </summary>
    Task<IReadOnlyList<DataScope>> GetUserDataScopesAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the user has full data access (All scope).
    /// </summary>
    Task<bool> HasFullAccessAsync(long userId, CancellationToken cancellationToken = default);
}
