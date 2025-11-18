using StandardWeb.Common.Auth;

namespace StandardWeb.Common.Helpers;

public class TokenClaimHelper
{
    public static long? GetUserIdFromClaims(System.Security.Claims.ClaimsPrincipal user)
    {
        if (user.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = user.FindFirst(JwtClaimTypes.UserId);
            if (userIdClaim != null && long.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }
        }
        return null; // Default value if no user is authenticated
    }
}