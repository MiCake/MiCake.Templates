using MiCake.EntityFrameworkCore.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using StandardWeb.Domain.Enums.Identity;
using StandardWeb.Domain.Models.Identity;

namespace StandardWeb.Domain.Repositories;

public class UserRepo(IServiceProvider serviceProvider) : EFRepositoryHasPaging<AppDbContext, User, long>(serviceProvider), IUserRepo
{
    public async Task<User?> GetByPhoneNumberAsync(string phoneNumber, bool needTracking = true, CancellationToken cancellationToken = default)
    {
        var query = needTracking ? DbSet.AsQueryable() : DbSet.AsNoTracking().AsQueryable();
        return await query.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber, cancellationToken);
    }

    public async Task<User?> FindByExternalLoginAsync(LoginProviderType providerType, string providerKey, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().Include(s => s.ExternalLogins)
        .Where(u => u.ExternalLogins.Any(e => e.ProviderType == providerType && e.ProviderKey == providerKey && !e.IsUnbound))
        .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<User?> FindByProviderUnionIdAsync(string providerUnionId, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().Include(s => s.ExternalLogins)
            .Where(u => u.ExternalLogins.Any(e => e.ProviderUnionId == providerUnionId && !e.IsUnbound))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<User?> GetByPhoneNumberWithIncludesAsync(string phoneNumber, Func<IQueryable<User>, IIncludableQueryable<User, object>>? include = null, bool needTracking = true, CancellationToken cancellationToken = default)
    {
        var query = needTracking ? DbSet.AsQueryable() : DbSet.AsNoTracking().AsQueryable();
        if (include != null)
        {
            query = include(query);
        }
        return await query.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber, cancellationToken);
    }

    public async Task<User?> GetByIdWithIncludesAsync(long id, Func<IQueryable<User>, IIncludableQueryable<User, object>>? include = null, bool needTracking = true, CancellationToken cancellationToken = default)
    {
        var query = needTracking ? DbSet.AsQueryable() : DbSet.AsNoTracking().AsQueryable();
        if (include != null)
        {
            query = include(query);
        }
        return await query.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public Task<User?> FindByUserTokenAsync(UserTokenType tokenType, string tokenValue, CancellationToken cancellationToken = default)
    {
        return DbSet.AsNoTracking().Include(s => s.UserTokens)
            .Where(u => u.UserTokens.Any(t => t.Type == tokenType && t.Value == tokenValue))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
