namespace StandardWeb.Common.Auth;

public class JwtTokenModel
{
    /// <summary>
    /// The generated JWT token.
    /// </summary>
    public string JwtToken { get; set; } = string.Empty;

    /// <summary>
    /// The expiration time of the JWT token.
    /// </summary>
    public DateTime Expiration { get; set; }

    /// <summary>
    /// The generated refresh token.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// The expiration time of the refresh token.
    /// </summary>
    public DateTime RefreshTokenExpiration { get; set; }
}
