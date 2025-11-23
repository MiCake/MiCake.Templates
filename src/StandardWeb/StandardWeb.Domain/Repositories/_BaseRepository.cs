using MiCake.DDD.Domain;
using MiCake.EntityFrameworkCore.Repository;
using Microsoft.EntityFrameworkCore;

namespace StandardWeb.Domain.Repositories;

public abstract class BaseRepository<TAggregateRoot> : EFRepository<AppDbContext, TAggregateRoot, long>
    where TAggregateRoot : class, IAggregateRoot<long>
{
    protected BaseRepository(EFRepositoryDependencies<AppDbContext> dependencies) : base(dependencies)
    {
    }

    protected IQueryable<TAggregateRoot> GetDbSet(bool needTracking)
    {
        return needTracking ? DbSet.AsQueryable() : DbSet.AsNoTracking().AsQueryable();
    }
}


public abstract class BasePagingRepository<TAggregateRoot> : EFRepositoryHasPaging<AppDbContext, TAggregateRoot, long>
    where TAggregateRoot : class, IAggregateRoot<long>
{
    protected BasePagingRepository(EFRepositoryDependencies<AppDbContext> dependencies) : base(dependencies)
    {
    }

    protected IQueryable<TAggregateRoot> GetDbSet(bool needTracking)
    {
        return needTracking ? DbSet.AsQueryable() : DbSet.AsNoTracking().AsQueryable();
    }
}