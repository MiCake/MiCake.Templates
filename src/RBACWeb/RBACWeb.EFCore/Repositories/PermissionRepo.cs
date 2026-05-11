using MiCake.EntityFrameworkCore.Repository;
using Microsoft.EntityFrameworkCore;
using RBACWeb.Domain.Models.Authorization;
using RBACWeb.Domain.Repositories;

namespace RBACWeb.EFCore.Repositories;

public class PermissionRepo : BasePagingRepository<Permission>, IPermissionRepo
{
    public PermissionRepo(EFRepositoryDependencies<AppDbContext> dependencies) : base(dependencies)
    {
    }

    public async Task<Permission?> GetByCodeAsync(string code, bool needTracking = true, CancellationToken cancellationToken = default)
    {
        return await GetDbSet(needTracking)
            .FirstOrDefaultAsync(p => p.Code == code, cancellationToken);
    }

    public async Task<IReadOnlyList<Permission>> GetByResourceIdAsync(long resourceId, CancellationToken cancellationToken = default)
    {
        return await GetDbSet(false)
            .Where(p => p.ResourceId == resourceId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Permission>> GetActivePermissionsAsync(CancellationToken cancellationToken = default)
    {
        return await GetDbSet(false)
            .Where(p => p.IsActive)
            .OrderBy(p => p.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Permission>> GetByIdsAsync(IEnumerable<long> ids, CancellationToken cancellationToken = default)
    {
        return await GetDbSet(false)
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Permission>> GetByCodesAsync(IEnumerable<string> codes, CancellationToken cancellationToken = default)
    {
        return await GetDbSet(false)
            .Where(p => codes.Contains(p.Code))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await GetDbSet(false)
            .AnyAsync(p => p.Code == code, cancellationToken);
    }
}
