using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RBACWeb.Common.Auth;
using RBACWeb.Common.Time;
using RBACWeb.Domain.Models.Identity;

namespace RBACWeb.Application.Providers;

[InjectService(Lifetime = MiCakeServiceLifetime.Scoped)]
public class JwtProvider(IOptions<JwtConfigOptions> jwtConfig)
{
    private readonly JwtConfigOptions _jwtConfig = jwtConfig?.Value ?? throw new ArgumentNullException(nameof(jwtConfig), "JWT configuration cannot be null.");

    public JwtTokenModel GenerateToken(User user, List<Claim> claims)
    {
        ArgumentNullException.ThrowIfNull(user);

        var securityKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_jwtConfig.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var finalClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtClaimTypes.UserId, user.Id.ToString()),
            new(JwtClaimTypes.PhoneNumber, user.PhoneNumber),
        };

        // Add role IDs to token - each role as a separate claim for proper Claims collection behavior
        var roleIds = user.GetEffectiveRoleIds();
        foreach (var roleId in roleIds)
        {
            finalClaims.Add(new Claim(JwtClaimTypes.Roles, roleId.ToString()));
        }

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
