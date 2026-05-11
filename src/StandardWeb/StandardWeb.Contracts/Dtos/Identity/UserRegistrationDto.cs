namespace StandardWeb.Contracts.Dtos.Identity;

/// <summary>
/// User registration request.
/// At least one contact method (PhoneNumber or Email) must be provided.
/// </summary>
public class UserRegistrationDto
{
    /// <summary>
    /// Phone number (optional if Email is provided)
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Email address (optional if PhoneNumber is provided)
    /// </summary>
    public string? Email { get; set; }

    public string? Password { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? DisplayName { get; set; }
}
