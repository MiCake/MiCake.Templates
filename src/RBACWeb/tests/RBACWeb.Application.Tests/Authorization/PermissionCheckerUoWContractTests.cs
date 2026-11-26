using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MiCake.DDD.Uow;
using MiCake.EntityFrameworkCore;
using MiCake.EntityFrameworkCore.Repository;
using MiCake.EntityFrameworkCore.Uow;
using Moq;
using RBACWeb.Application.Authorization;
using RBACWeb.Application.Cache;
using RBACWeb.EFCore;
using RBACWeb.EFCore.Repositories;

namespace RBACWeb.Application.Tests.Authorization;

/// <summary>
/// Reproduces the UoW contract behind the RBAC permission query path.
/// The permission checker queries repositories that resolve the DbContext through
/// <see cref="EFCoreContextFactory{TDbContext}"/>; without an ambient unit of work the
/// factory rejects access unless <see cref="MiCakeEFCoreOptions.AllowDbContextAccessWithoutUoW"/>
/// is enabled. The authorization handler runs before MiCake's action-filter UoW is created,
/// so this is the exact environment a real permission check encounters.
/// </summary>
public class PermissionCheckerUoWContractTests
{
    [Fact]
    public async Task permission_query_without_uow_throws_by_default()
    {
        var checker = BuildPermissionChecker(allowAccessWithoutUoW: false);

        // Cache miss forces a repository query -> context resolution -> no ambient UoW.
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => checker.HasPermissionAsync(userId: 1, "user:list"));

        Assert.Contains("No active Unit of Work", exception.Message);
    }

    [Fact]
    public async Task permission_query_without_uow_succeeds_when_access_is_allowed()
    {
        var checker = BuildPermissionChecker(allowAccessWithoutUoW: true);

        // Relaxed resolution permits the read-only permission query (empty store -> no permission).
        var hasPermission = await checker.HasPermissionAsync(userId: 1, "user:list");

        Assert.False(hasPermission);
    }

    private static PermissionChecker BuildPermissionChecker(bool allowAccessWithoutUoW)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        var provider = services.BuildServiceProvider();

        var unitOfWorkManager = new Mock<IUnitOfWorkManager>();
        unitOfWorkManager.Setup(manager => manager.Current).Returns((IUnitOfWork)null);

        var options = new MiCakeEFCoreOptions(typeof(AppDbContext))
        {
            AllowDbContextAccessWithoutUoW = allowAccessWithoutUoW
        };
        var contextFactory = new EFCoreContextFactory<AppDbContext>(
            provider,
            unitOfWorkManager.Object,
            provider.GetRequiredService<ILogger<EFCoreContextFactory<AppDbContext>>>(),
            options);
        var dependencies = new EFRepositoryDependencies<AppDbContext>(
            contextFactory,
            unitOfWorkManager.Object,
            provider.GetRequiredService<ILogger<EFRepositoryDependencies<AppDbContext>>>(),
            options);
        var cacheService = new CacheService(
            new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions())),
            provider.GetRequiredService<ILogger<CacheService>>());

        return new PermissionChecker(
            new UserRepo(dependencies),
            new RoleRepo(dependencies),
            cacheService,
            provider.GetRequiredService<ILogger<PermissionChecker>>());
    }
}
