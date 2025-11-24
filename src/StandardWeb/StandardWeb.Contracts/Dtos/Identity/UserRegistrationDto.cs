namespace StandardWeb.Contracts.Dtos.Identity;

public class UserRegistrationDto
{
    public string? PhoneNumber { get; set; }
    public string? Password { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? DisplayName { get; set; }
}
