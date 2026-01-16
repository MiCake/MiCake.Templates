using MiCake.EntityFrameworkCore.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using StandardWeb.Domain.Enums.Identity;
using StandardWeb.Domain.Models.Identity;

namespace StandardWeb.Domain.Repositories;

public class UserRepo : BasePagingRepository<User>, IUserRepo
{
    public UserRepo(EFRepositoryDependencies<AppDbContext> dependencies) : base(dependencies)
    {
    }

    public async Task<User?> GetByPhoneNumberAsync(string phoneNumber, bool needTracking = true, CancellationToken cancellationToken = default)
    {
        return await GetDbSet(needTracking).FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber, cancellationToken);
    }

    public async Task<User?> FindByExternalLoginAsync(LoginProviderType providerType, string providerKey, bool needTracking = true, CancellationToken cancellationToken = default)
    {
        return await GetDbSet(needTracking).Include(s => s.ExternalLogins)
        .Where(u => u.ExternalLogins.Any(e => e.ProviderType == providerType && e.ProviderKey == providerKey && !e.IsUnbound))
        .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<User?> FindByProviderUnionIdAsync(string providerUnionId, bool needTracking = true, CancellationToken cancellationToken = default)
    {
        return await GetDbSet(needTracking).Include(s => s.ExternalLogins)
            .Where(u => u.ExternalLogins.Any(e => e.ProviderUnionId == providerUnionId && !e.IsUnbound))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<User?> GetByPhoneNumberWithIncludesAsync(string phoneNumber, Func<IQueryable<User>, IIncludableQueryable<User, object>>? include = null, bool needTracking = true, CancellationToken cancellationToken = default)
    {
        var query = GetDbSet(needTracking);
        if (include != null)
        {
            query = include(query);
        }
        return await query.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber, cancellationToken);
    }

    public async Task<User?> GetByIdWithIncludesAsync(long id, Func<IQueryable<User>, IIncludableQueryable<User, object>>? include = null, bool needTracking = true, CancellationToken cancellationToken = default)
    {
        var query = GetDbSet(needTracking);
        if (include != null)
        {
            query = include(query);
        }
        return await query.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public Task<User?> FindByUserTokenAsync(UserTokenType tokenType, string tokenValue, bool needTracking = true, CancellationToken cancellationToken = default)
    {
        return GetDbSet(needTracking).Include(s => s.UserTokens)
                    .Where(u => u.UserTokens.Any(t => t.Type == tokenType && t.Value == tokenValue))
                    .FirstOrDefaultAsync(cancellationToken);
    }
}
