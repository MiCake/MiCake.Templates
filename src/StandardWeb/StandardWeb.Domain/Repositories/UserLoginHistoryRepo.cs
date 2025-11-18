using MiCake.EntityFrameworkCore.Repository;
using Microsoft.EntityFrameworkCore;
using StandardWeb.Domain.Models.Identity;

namespace StandardWeb.Domain.Repositories;

public class UserLoginHistoryRepo(IServiceProvider serviceProvider) : EFRepositoryHasPaging<AppDbContext, UserLoginHistory, long>(serviceProvider), IUserLoginHistoryRepo
{
    public async Task<List<UserLoginHistory>> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(h => h.UserId == userId).ToListAsync(cancellationToken);
    }
}