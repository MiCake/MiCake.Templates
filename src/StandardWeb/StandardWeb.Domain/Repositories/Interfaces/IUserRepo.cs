using MiCake.DDD.Domain;
using Microsoft.EntityFrameworkCore.Query;
using StandardWeb.Domain.Enums.Identity;
using StandardWeb.Domain.Models.Identity;

namespace StandardWeb.Domain.Repositories;

public interface IUserRepo : IRepositoryHasPagingQuery<User, long>
{
    Task<User?> GetByPhoneNumberAsync(string phoneNumber, bool needTracking = true, CancellationToken cancellationToken = default);

    Task<User?> GetByPhoneNumberWithIncludesAsync(string phoneNumber, Func<IQueryable<User>, IIncludableQueryable<User, object>>? include = null, bool needTracking = true, CancellationToken cancellationToken = default);

    Task<User?> GetByIdWithIncludesAsync(long id, Func<IQueryable<User>, IIncludableQueryable<User, object>>? include = null, bool needTracking = true, CancellationToken cancellationToken = default);

    #region Query for inner entities

    Task<User?> FindByExternalLoginAsync(LoginProviderType providerType, string providerKey, bool needTracking = true, CancellationToken cancellationToken = default);
    Task<User?> FindByProviderUnionIdAsync(string providerUnionId, bool needTracking = true, CancellationToken cancellationToken = default);

    Task<User?> FindByUserTokenAsync(UserTokenType tokenType, string tokenValue, bool needTracking = true, CancellationToken cancellationToken = default);
    #endregion
}