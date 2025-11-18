using StandardWeb.Domain.Models.Identity;

namespace StandardWeb.Application.Models;

public class UserLoginResult
{
    public bool LoginPassed { get; set; }
    public User User { get; set; } = null!;
    public string Token { get; set; } = null!;
    public DateTime Expiration { get; set; }
    public bool NeedOtpVerification { get; set; }

    public string RefreshToken { get; set; } = null!;
    public DateTime RefreshTokenExpiration { get; set; }
}

public class UserLoginModel
{
    public string? PhoneNumber { get; set; }
    public string? Password { get; set; }
    public string? OtpCode { get; set; }
}

public class UserRegistrationModel
{
    public string? PhoneNumber { get; set; }
    public string? Password { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? DisplayName { get; set; }
}