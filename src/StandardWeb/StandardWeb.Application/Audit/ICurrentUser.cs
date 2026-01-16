using Microsoft.AspNetCore.Http;
using StandardWeb.Common.Auth;

namespace StandardWeb.Application.Audit;

/// <summary>
/// A service to get information about the current user from JWT claims.
/// </summary>
public interface ICurrentUser
{
    long? GetCurrentUserId();
}

[InjectService(typeof(ICurrentUser), Lifetime = MiCakeServiceLifetime.Scoped)]
internal class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public long? GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(JwtClaimTypes.UserId);
        if (userIdClaim != null && long.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }
        return null;
    }
}