using StandardWeb.Common.Auth;

namespace StandardWeb.Common.Helpers;

/// <summary>
/// Provides utility methods for extracting information from JWT token claims
/// </summary>
public class TokenClaimHelper
{
    /// <summary>
    /// Extracts the user ID from the authenticated user's JWT claims
    /// </summary>
    /// <param name="user">The claims principal representing the authenticated user</param>
    /// <returns>The user ID if found and valid; otherwise null</returns>
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
        return null;
    }
}