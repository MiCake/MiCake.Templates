using MiCake.EntityFrameworkCore.Repository;
using Microsoft.EntityFrameworkCore;
using StandardWeb.Domain.Models.Identity;

namespace StandardWeb.Domain.Repositories;

public class UserLoginHistoryRepo : BasePagingRepository<UserLoginHistory>, IUserLoginHistoryRepo
{
    public UserLoginHistoryRepo(EFRepositoryDependencies<AppDbContext> dependencies) : base(dependencies)
    {
    }

    public async Task<List<UserLoginHistory>> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(h => h.UserId == userId).ToListAsync(cancellationToken);
    }
}