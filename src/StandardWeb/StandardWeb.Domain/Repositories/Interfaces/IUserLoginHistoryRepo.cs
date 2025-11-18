using MiCake.DDD.Extensions.Paging;
using StandardWeb.Domain.Models.Identity;

namespace StandardWeb.Domain.Repositories;

public interface IUserLoginHistoryRepo : IRepositoryHasPagingQuery<UserLoginHistory, long>
{
    Task<List<UserLoginHistory>> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default);
}