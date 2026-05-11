using MiCake.EntityFrameworkCore.Repository;
using Microsoft.EntityFrameworkCore;
using RBACWeb.Domain.Enums.Authorization;
using RBACWeb.Domain.Models.Authorization;
using RBACWeb.Domain.Repositories;

namespace RBACWeb.EFCore.Repositories;

public class DataScopeRepo : BasePagingRepository<DataScope>, IDataScopeRepo
{
    public DataScopeRepo(EFRepositoryDependencies<AppDbContext> dependencies) : base(dependencies)
    {
    }

    public async Task<DataScope?> GetByCodeAsync(string code, bool needTracking = true, CancellationToken cancellationToken = default)
    {
        return await GetDbSet(needTracking)
            .FirstOrDefaultAsync(ds => ds.Code == code, cancellationToken);
    }

    public async Task<IReadOnlyList<DataScope>> GetByTypeAsync(DataScopeType type, CancellationToken cancellationToken = default)
    {
        return await GetDbSet(false)
            .Where(ds => ds.Type == type && ds.IsActive)
            .OrderByDescending(ds => ds.Priority)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DataScope>> GetActiveDataScopesAsync(CancellationToken cancellationToken = default)
    {
        return await GetDbSet(false)
            .Where(ds => ds.IsActive)
            .OrderByDescending(ds => ds.Priority)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DataScope>> GetByIdsAsync(IEnumerable<long> ids, CancellationToken cancellationToken = default)
    {
        return await GetDbSet(false)
            .Where(ds => ids.Contains(ds.Id))
            .OrderByDescending(ds => ds.Priority)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await GetDbSet(false)
            .AnyAsync(ds => ds.Code == code, cancellationToken);
    }
}
