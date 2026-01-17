using RBACWeb.Domain.Models.Identity;

namespace RBACWeb.Domain.Repositories;

public interface IUserLoginHistoryRepo : IRepositoryHasPagingQuery<UserLoginHistory, long>
{
    Task<List<UserLoginHistory>> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default);
}