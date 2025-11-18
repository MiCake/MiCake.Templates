namespace StandardWeb.Common.Auth;

public class JwtTokenModel
{
    public string JwtToken { get; set; } = string.Empty;

    public DateTime Expiration { get; set; }

    public string RefreshToken { get; set; } = string.Empty;

    public DateTime RefreshTokenExpiration { get; set; }
}
