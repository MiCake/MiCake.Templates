using System.Security.Claims;
using MiCake.Core.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StandardWeb.Common.Auth;
using StandardWeb.Common.Time;
using StandardWeb.Domain.Models.Identity;

namespace StandardWeb.Application.Providers;

/// <summary>
/// Provides JWT token generation and management services
/// </summary>
[InjectService(Lifetime = MiCakeServiceLifetime.Scoped)]
public class JwtProvider(IOptions<JwtConfigOptions> jwtConfig)
{
    private readonly JwtConfigOptions _jwtConfig = jwtConfig?.Value ?? throw new ArgumentNullException(nameof(jwtConfig), "JWT configuration cannot be null.");

    /// <summary>
    /// Generates a JWT access token and refresh token for the specified user
    /// </summary>
    /// <param name="user">The user for whom to generate the token</param>
    /// <param name="claims">Additional claims to include in the token</param>
    /// <returns>A JwtTokenModel containing the access token, refresh token, and expiration times</returns>
    public JwtTokenModel GenerateToken(User user, List<Claim> claims)
    {
        if (user == null) throw new ArgumentNullException(nameof(user), "User cannot be null.");

        var securityKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_jwtConfig.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var finalClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtClaimTypes.UserId, user.Id.ToString()),
            new(JwtClaimTypes.PhoneNumber, user.PhoneNumber),
        };

        if (claims != null && claims.Count > 0)
        {
            finalClaims.AddRange(claims);
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(finalClaims),
            Expires = TimeNow.Now.AddMinutes(_jwtConfig.ExpirationMinutes),
            Issuer = _jwtConfig.Issuer,
            Audience = _jwtConfig.Audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        var jwtToken = tokenHandler.WriteToken(token);
        var refreshToken = GenerateRefreshToken();

        return new JwtTokenModel
        {
            JwtToken = jwtToken,
            Expiration = TimeNow.Now.AddMinutes(_jwtConfig.ExpirationMinutes),
            RefreshToken = refreshToken,
            RefreshTokenExpiration = TimeNow.Now.AddMinutes(_jwtConfig.RefreshTokenExpirationMinutes)
        };
    }

    /// <summary>
    /// Extracts claims from a JWT token for validation and token generation
    /// </summary>
    /// <param name="token">The JWT token string to parse</param>
    /// <returns>A list of claims extracted from the token, or an empty list if parsing fails</returns>
    public static List<Claim> GetClaimsFromToken(string token)
    {
        try
        {
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            return jwtToken.Claims.ToList();
        }
        catch
        {
            return new List<Claim>();
        }
    }

    /// <summary>
    /// Generates a cryptographically secure random refresh token
    /// </summary>
    /// <returns>A base64-encoded random string suitable for use as a refresh token</returns>
    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
        }
        return Convert.ToBase64String(randomNumber);
    }
}
