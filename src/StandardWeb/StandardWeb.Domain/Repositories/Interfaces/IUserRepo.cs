using MiCake.DDD.Extensions.Paging;
using Microsoft.EntityFrameworkCore.Query;
using StandardWeb.Domain.Enums.Identity;
using StandardWeb.Domain.Models.Identity;

namespace StandardWeb.Domain.Repositories;

/// <summary>
/// Repository interface for User aggregate providing user-specific query operations
/// </summary>
public interface IUserRepo : IRepositoryHasPagingQuery<User, long>
{
    /// <summary>
    /// Retrieves a user by phone number
    /// </summary>
    /// <param name="phoneNumber">The phone number to search for</param>
    /// <param name="needTracking">Whether to track the entity for updates</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User if found; otherwise null</returns>
    Task<User?> GetByPhoneNumberAsync(string phoneNumber, bool needTracking = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a user by phone number with related entities eagerly loaded
    /// </summary>
    /// <param name="phoneNumber">The phone number to search for</param>
    /// <param name="include">Function to specify related entities to include</param>
    /// <param name="needTracking">Whether to track the entity for updates</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User with related entities if found; otherwise null</returns>
    Task<User?> GetByPhoneNumberWithIncludesAsync(string phoneNumber, Func<IQueryable<User>, IIncludableQueryable<User, object>>? include = null, bool needTracking = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a user by ID with related entities eagerly loaded
    /// </summary>
    /// <param name="id">The user ID to search for</param>
    /// <param name="include">Function to specify related entities to include</param>
    /// <param name="needTracking">Whether to track the entity for updates</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User with related entities if found; otherwise null</returns>
    Task<User?> GetByIdWithIncludesAsync(long id, Func<IQueryable<User>, IIncludableQueryable<User, object>>? include = null, bool needTracking = true, CancellationToken cancellationToken = default);

    #region Query for inner entities which does not need to be tracked by efcore

    /// <summary>
    /// Finds a user by external login provider information
    /// </summary>
    /// <param name="providerType">The type of external login provider</param>
    /// <param name="providerKey">The unique key from the external provider</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User if found; otherwise null</returns>
    Task<User?> FindByExternalLoginAsync(LoginProviderType providerType, string providerKey, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Finds a user by external provider union ID
    /// </summary>
    /// <param name="providerUnionId">The union ID from the external provider</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User if found; otherwise null</returns>
    Task<User?> FindByProviderUnionIdAsync(string providerUnionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a user by their token value
    /// </summary>
    /// <param name="tokenType">The type of token to search for</param>
    /// <param name="tokenValue">The token value to search for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User if found; otherwise null</returns>
    Task<User?> FindByUserTokenAsync(UserTokenType tokenType, string tokenValue, CancellationToken cancellationToken = default);
    #endregion
}