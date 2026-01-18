namespace RBACWeb.Contracts.Dtos.Identity;

/// <summary>
/// Login request DTO.
/// PhoneNumber can contain either a phone number or email address.
/// </summary>
public class LoginRequestDto
{
    public string? PhoneNumber { get; set; }

    public string? Password { get; set; }
    public string? OtpCode { get; set; }
}
