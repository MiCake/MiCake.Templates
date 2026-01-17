using MiCake.EntityFrameworkCore.Repository;
using Microsoft.EntityFrameworkCore;
using RBACWeb.Domain.Enums.Authorization;
using RBACWeb.Domain.Models.Authorization;
using RBACWeb.Domain.Repositories;

namespace RBACWeb.EFCore.Repositories;

public class ResourceRepo : BasePagingRepository<Resource>, IResourceRepo
{
    public ResourceRepo(EFRepositoryDependencies<AppDbContext> dependencies) : base(dependencies)
    {
    }

    public async Task<Resource?> GetByCodeAsync(string code, bool needTracking = true, CancellationToken cancellationToken = default)
    {
        return await GetDbSet(needTracking)
            .FirstOrDefaultAsync(r => r.Code == code, cancellationToken);
    }

    public async Task<IReadOnlyList<Resource>> GetByTypeAsync(ResourceType type, CancellationToken cancellationToken = default)
    {
        return await GetDbSet(false)
            .Where(r => r.Type == type && r.IsActive)
            .OrderBy(r => r.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Resource>> GetChildrenAsync(long parentId, CancellationToken cancellationToken = default)
    {
        return await GetDbSet(false)
            .Where(r => r.ParentId == parentId && r.IsActive)
            .OrderBy(r => r.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Resource>> GetRootResourcesAsync(CancellationToken cancellationToken = default)
    {
        return await GetDbSet(false)
            .Where(r => r.ParentId == null && r.IsActive)
            .OrderBy(r => r.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Resource>> GetActiveResourcesAsync(CancellationToken cancellationToken = default)
    {
        return await GetDbSet(false)
            .Where(r => r.IsActive)
            .OrderBy(r => r.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<Resource?> GetWithPermissionsAsync(long id, bool needTracking = true, CancellationToken cancellationToken = default)
    {
        return await GetDbSet(needTracking)
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await GetDbSet(false)
            .AnyAsync(r => r.Code == code, cancellationToken);
    }
}
