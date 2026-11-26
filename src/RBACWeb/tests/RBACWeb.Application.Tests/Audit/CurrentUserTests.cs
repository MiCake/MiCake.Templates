using Microsoft.AspNetCore.Http;
using Moq;
using RBACWeb.Application.Audit;
using RBACWeb.Application.Authorization;
using RBACWeb.Common.Auth;
using System.Security.Claims;

namespace RBACWeb.Application.Tests.Audit;

/// <summary>
/// Tests for <see cref="CurrentUser"/> JWT claim parsing.
/// The permission authorization handler relies on these parsing rules
/// to decide whether a permission lookup is performed.
/// </summary>
public class CurrentUserTests
{
    private static CurrentUser CreateCurrentUser(params Claim[] claims)
    {
        var identity = new ClaimsIdentity(claims, authenticationType: "test");
        var principal = new ClaimsPrincipal(identity);
        var accessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
        return new CurrentUser(
            accessor,
            new Mock<IPermissionChecker>().Object,
            new Mock<IDataScopeResolver>().Object);
    }

    [Fact]
    public void GetCurrentUserId_returns_id_when_userid_claim_is_valid()
    {
        var currentUser = CreateCurrentUser(new Claim(JwtClaimTypes.UserId, "42"));

        Assert.Equal(42, currentUser.GetCurrentUserId());
    }

    [Fact]
    public void GetCurrentUserId_returns_null_when_userid_claim_is_missing()
    {
        var currentUser = CreateCurrentUser();

        Assert.Null(currentUser.GetCurrentUserId());
    }

    [Fact]
    public void GetCurrentUserId_returns_null_when_userid_claim_is_not_a_number()
    {
        var currentUser = CreateCurrentUser(new Claim(JwtClaimTypes.UserId, "not-a-number"));

        Assert.Null(currentUser.GetCurrentUserId());
    }

    [Fact]
    public void GetRoleIds_parses_all_valid_role_claims_and_skips_invalid_ones()
    {
        var currentUser = CreateCurrentUser(
            new Claim(JwtClaimTypes.Roles, "1"),
            new Claim(JwtClaimTypes.Roles, "2"),
            new Claim(JwtClaimTypes.Roles, "not-a-number"));

        Assert.Equal(new long[] { 1, 2 }, currentUser.GetRoleIds());
    }

    [Fact]
    public void GetRoleIds_returns_empty_when_no_role_claims_exist()
    {
        var currentUser = CreateCurrentUser(new Claim(JwtClaimTypes.UserId, "1"));

        Assert.Empty(currentUser.GetRoleIds());
    }
}
