using StandardWeb.Common.Auth;

namespace StandardWeb.Common.Helpers;

/// <summary>
/// Provides helper methods for extracting claims from JWT tokens.
/// </summary>
public class TokenClaimHelper
{
    /// <summary>
    /// Extracts the user ID from JWT token claims.
    /// Used by controllers to identify the authenticated user making the request.
    /// </summary>
    /// <param name="user">ClaimsPrincipal from the current HTTP context</param>
    /// <returns>User ID if authenticated and claim exists, null otherwise</returns>
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
        return null; // Return null if user is not authenticated or claim is missing
    }
}