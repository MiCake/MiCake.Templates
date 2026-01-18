using StandardWeb.Domain.Enums.Identity;
using StandardWeb.Domain.Models.Identity;

namespace StandardWeb.Domain.Repositories;

public interface IUserRepo : IRepositoryHasPagingQuery<User, long>
{
    /// <summary>
    /// Finds a user by phone number or email (primary contact lookup).
    /// </summary>
    Task<User?> FindByContactAsync(string phoneOrEmail, bool needTracking = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by phone number (legacy method for backward compatibility).
    /// </summary>
    Task<User?> GetByPhoneNumberAsync(string phoneNumber, bool needTracking = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by email address.
    /// </summary>
    Task<User?> GetByEmailAsync(string email, bool needTracking = true, CancellationToken cancellationToken = default);

    Task<User?> GetByPhoneNumberWithIncludesAsync(string phoneNumber, Func<IQueryable<User>, IQueryable<User>>? include = null, bool needTracking = true, CancellationToken cancellationToken = default);

    Task<User?> GetByIdWithIncludesAsync(long id, Func<IQueryable<User>, IQueryable<User>>? include = null, bool needTracking = true, CancellationToken cancellationToken = default);

    #region Query for inner entities

    Task<User?> FindByExternalLoginAsync(LoginProviderType providerType, string providerKey, bool needTracking = true, CancellationToken cancellationToken = default);
    Task<User?> FindByProviderUnionIdAsync(string providerUnionId, bool needTracking = true, CancellationToken cancellationToken = default);

    Task<User?> FindByUserTokenAsync(UserTokenType tokenType, string tokenValue, bool needTracking = true, CancellationToken cancellationToken = default);
    #endregion
}