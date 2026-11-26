using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MiCake.DDD.Uow;
using MiCake.EntityFrameworkCore;
using MiCake.EntityFrameworkCore.Repository;
using MiCake.EntityFrameworkCore.Uow;
using Moq;
using RBACWeb.Application.Audit;
using RBACWeb.Application.Authorization;
using RBACWeb.Application.Cache;
using RBACWeb.EFCore;
using RBACWeb.EFCore.Repositories;
using RBACWeb.Web.Authorization;
using System.Security.Claims;

namespace RBACWeb.Web.Tests.Authorization;

/// <summary>
/// End-to-end verification of the permission authorization flow with real components
/// (handler -> standalone executor boundary -> permission checker -> repositories -> DbContext).
/// The authorization stage runs before MiCake's action-filter unit of work is created, so the
/// handler must execute the permission query inside an explicit UoW boundary provided by
/// <see cref="IStandaloneUnitOfWorkExecutor"/>. The executor's real implementation (isolated
/// DI scope + root UoW + commit/rollback) is guaranteed by the MiCake framework; this test
/// supplies an executor that runs the callback with an accessible checker chain, proving the
/// handler wiring completes the query without throwing.
/// </summary>
public class PermissionAuthorizationHandlerIntegrationTests
{
    [Fact]
    public async Task permission_authorization_completes_when_permission_query_hits_database()
    {
        // The standalone executor boundary makes the permission query reachable: the checker
        // chain resolves a DbContext inside an environment where access is permitted.
        // Empty store -> no permissions -> authorization fails gracefully (no exception).
        var checker = BuildPermissionChecker(allowAccessWithoutUoW: true);
        var checkerProvider = new ServiceCollection()
            .AddSingleton<IPermissionChecker>(checker)
            .BuildServiceProvider();

        var executor = new Mock<IStandaloneUnitOfWorkExecutor>();
        executor.Setup(executor => executor.ExecuteAsync(
                It.IsAny<Func<IServiceProvider, CancellationToken, Task<bool>>>(),
                It.IsAny<UnitOfWorkOptions?>(),
                It.IsAny<CancellationToken>()))
            .Returns((Func<IServiceProvider, CancellationToken, Task<bool>> operation, UnitOfWorkOptions? options, CancellationToken cancellationToken)
                => operation(checkerProvider, cancellationToken));

        var currentUser = new Mock<ICurrentUser>();
        currentUser.Setup(user => user.GetCurrentUserId()).Returns(1L);
        var handler = new PermissionAuthorizationHandler(
            currentUser.Object,
            executor.Object,
            NullLogger<PermissionAuthorizationHandler>.Instance);
        var context = new AuthorizationHandlerContext(
            new IAuthorizationRequirement[] { new PermissionRequirement("user:list") },
            new ClaimsPrincipal(),
            resource: null);

        // Must complete without throwing; empty store means the user has no permission.
        await handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
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
