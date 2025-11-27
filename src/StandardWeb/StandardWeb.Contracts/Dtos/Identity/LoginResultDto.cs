namespace StandardWeb.Contracts.Dtos.Identity;

public class LoginResultDto
{
    public bool LoginPassed { get; set; }
    public UserDto? User { get; set; }
    public string? Token { get; set; }
    public DateTime Expiration { get; set; }
    public bool NeedOtpVerification { get; set; }

    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiration { get; set; }
}
