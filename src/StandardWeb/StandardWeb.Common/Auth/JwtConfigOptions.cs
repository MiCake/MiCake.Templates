namespace StandardWeb.Common.Auth;

public class JwtConfigOptions
{
    /// <summary>
    /// The secret key used for signing the JWT tokens.
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// The issuer of the JWT tokens.
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// The audience for which the JWT tokens are intended.
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// The expiration time for the JWT tokens in minutes.
    /// </summary>
    public int ExpirationMinutes { get; set; } = 60;

    /// <summary>
    /// The expiration time for the refresh tokens in minutes.
    /// </summary>
    public int RefreshTokenExpirationMinutes { get; set; } = 1440;

}
