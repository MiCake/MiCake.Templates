namespace RBACWeb.Contracts.Dtos.Identity;

public class LoginResultDto
{
    public bool LoginPassed { get; set; }
    public UserDto? User { get; set; }
    public string? Token { get; set; }
    public DateTimeOffset Expiration { get; set; }
    public bool NeedOtpVerification { get; set; }

    public string? RefreshToken { get; set; }
    public DateTimeOffset RefreshTokenExpiration { get; set; }
}
