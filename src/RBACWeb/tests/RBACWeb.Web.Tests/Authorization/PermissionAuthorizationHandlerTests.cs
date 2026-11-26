using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using MiCake.DDD.Uow;
using Moq;
using RBACWeb.Application.Audit;
using RBACWeb.Web.Authorization;
using System.Security.Claims;

namespace RBACWeb.Web.Tests.Authorization;

/// <summary>
/// Unit tests for <see cref="PermissionAuthorizationHandler"/> decision branches
/// with a stubbed current user.
/// </summary>
public class PermissionAuthorizationHandlerTests
{
    [Fact]
    public async Task anonymous_user_is_not_authorized_and_no_permission_query_is_issued()
    {
        var currentUser = new Mock<ICurrentUser>();
        currentUser.Setup(user => user.GetCurrentUserId()).Returns((long?)null);
        var (handler, executor) = CreateHandler(currentUser.Object, permissionResult: true);
        var context = CreateContext();

        await handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
        executor.Verify(executor => executor.ExecuteAsync(
            It.IsAny<Func<IServiceProvider, CancellationToken, Task<bool>>>(),
            It.IsAny<UnitOfWorkOptions?>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task user_with_permission_is_authorized()
    {
        var currentUser = new Mock<ICurrentUser>();
        currentUser.Setup(user => user.GetCurrentUserId()).Returns(1L);
        var (handler, executor) = CreateHandler(currentUser.Object, permissionResult: true);
        var context = CreateContext();

        await handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
        executor.Verify(executor => executor.ExecuteAsync(
            It.IsAny<Func<IServiceProvider, CancellationToken, Task<bool>>>(),
            It.IsAny<UnitOfWorkOptions?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task user_without_permission_is_not_authorized()
    {
        var currentUser = new Mock<ICurrentUser>();
        currentUser.Setup(user => user.GetCurrentUserId()).Returns(1L);
        var (handler, executor) = CreateHandler(currentUser.Object, permissionResult: false);
        var context = CreateContext();

        await handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
        executor.Verify(executor => executor.ExecuteAsync(
            It.IsAny<Func<IServiceProvider, CancellationToken, Task<bool>>>(),
            It.IsAny<UnitOfWorkOptions?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task user_with_null_user_id_is_treated_as_anonymous()
    {
        var currentUser = new Mock<ICurrentUser>();
        currentUser.Setup(user => user.GetCurrentUserId()).Returns((long?)null);
        var (handler, executor) = CreateHandler(currentUser.Object, permissionResult: true);
        var context = CreateContext();

        await handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
        executor.Verify(executor => executor.ExecuteAsync(
            It.IsAny<Func<IServiceProvider, CancellationToken, Task<bool>>>(),
            It.IsAny<UnitOfWorkOptions?>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    private static (PermissionAuthorizationHandler Handler, Mock<IStandaloneUnitOfWorkExecutor> Executor)
        CreateHandler(ICurrentUser currentUser, bool permissionResult)
    {
        var executor = new Mock<IStandaloneUnitOfWorkExecutor>();
        executor.Setup(executor => executor.ExecuteAsync(
                It.IsAny<Func<IServiceProvider, CancellationToken, Task<bool>>>(),
                It.IsAny<UnitOfWorkOptions?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissionResult);

        var handler = new PermissionAuthorizationHandler(
            currentUser,
            executor.Object,
            NullLogger<PermissionAuthorizationHandler>.Instance);
        return (handler, executor);
    }

    private static AuthorizationHandlerContext CreateContext()
    {
        return new AuthorizationHandlerContext(
            new IAuthorizationRequirement[] { new PermissionRequirement("user:list") },
            new ClaimsPrincipal(),
            resource: null);
    }
}
