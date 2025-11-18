using MiCake.DDD.Extensions.Paging;
using Microsoft.EntityFrameworkCore.Query;
using StandardWeb.Domain.Enums.Identity;
using StandardWeb.Domain.Models.Identity;

namespace StandardWeb.Domain.Repositories;

public interface IUserRepo : IRepositoryHasPagingQuery<User, long>
{
    Task<User?> GetByPhoneNumberAsync(string phoneNumber, bool needTracking = true, CancellationToken cancellationToken = default);

    Task<User?> GetByPhoneNumberWithIncludesAsync(string phoneNumber, Func<IQueryable<User>, IIncludableQueryable<User, object>>? include = null, bool needTracking = true, CancellationToken cancellationToken = default);

    Task<User?> GetByIdWithIncludesAsync(long id, Func<IQueryable<User>, IIncludableQueryable<User, object>>? include = null, bool needTracking = true, CancellationToken cancellationToken = default);

    #region Query for inner entities which does not need to be tracked by efcore

    Task<User?> FindByExternalLoginAsync(LoginProviderType providerType, string providerKey, CancellationToken cancellationToken = default);
    Task<User?> FindByProviderUnionIdAsync(string providerUnionId, CancellationToken cancellationToken = default);

    Task<User?> FindByUserTokenAsync(UserTokenType tokenType, string tokenValue, CancellationToken cancellationToken = default);
    #endregion
}