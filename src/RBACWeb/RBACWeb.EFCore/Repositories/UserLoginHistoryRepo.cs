using MiCake.EntityFrameworkCore.Repository;
using Microsoft.EntityFrameworkCore;
using RBACWeb.Domain.Models.Identity;
using RBACWeb.Domain.Repositories;

namespace RBACWeb.EFCore.Repositories;

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