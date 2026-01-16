namespace StandardWeb.Contracts.Dtos.Identity;

public class LoginRequestDto
{
    public string? PhoneNumber { get; set; }
    public string? Password { get; set; }
    public string? OtpCode { get; set; }
}
