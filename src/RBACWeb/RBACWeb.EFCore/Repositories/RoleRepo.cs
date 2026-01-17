using MiCake.EntityFrameworkCore.Repository;
using Microsoft.EntityFrameworkCore;
using RBACWeb.Domain.Models.Authorization;
using RBACWeb.Domain.Repositories;

namespace RBACWeb.EFCore.Repositories;

public class RoleRepo : BasePagingRepository<Role>, IRoleRepo
{
    public RoleRepo(EFRepositoryDependencies<AppDbContext> dependencies) : base(dependencies)
    {
    }

    public async Task<Role?> GetByCodeAsync(string code, bool needTracking = true, CancellationToken cancellationToken = default)
    {
        return await GetDbSet(needTracking)
            .FirstOrDefaultAsync(r => r.Code == code, cancellationToken);
    }

    public async Task<Role?> GetWithPermissionsAsync(long id, bool needTracking = true, CancellationToken cancellationToken = default)
    {
        return await GetDbSet(needTracking)
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Role?> GetWithDataScopesAsync(long id, bool needTracking = true, CancellationToken cancellationToken = default)
    {
        return await GetDbSet(needTracking)
            .Include(r => r.RoleDataScopes)
                .ThenInclude(rds => rds.DataScope)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Role?> GetWithAllIncludesAsync(long id, bool needTracking = true, CancellationToken cancellationToken = default)
    {
        return await GetDbSet(needTracking)
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .Include(r => r.RoleDataScopes)
                .ThenInclude(rds => rds.DataScope)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Role>> GetActiveRolesAsync(CancellationToken cancellationToken = default)
    {
        return await GetDbSet(false)
            .Where(r => r.IsActive)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Role>> GetByIdsWithPermissionsAsync(IEnumerable<long> roleIds, CancellationToken cancellationToken = default)
    {
        return await GetDbSet(false)
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .Include(r => r.RoleDataScopes)
                .ThenInclude(rds => rds.DataScope)
            .Where(r => roleIds.Contains(r.Id) && r.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await GetDbSet(false)
            .AnyAsync(r => r.Code == code, cancellationToken);
    }
}
