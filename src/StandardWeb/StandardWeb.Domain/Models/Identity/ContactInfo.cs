using System.ComponentModel.DataAnnotations;
using StandardWeb.Common.Helpers;

namespace StandardWeb.Domain.Models.Identity;

/// <summary>
/// Contact information value object - at least one contact method is required (phone or email).
/// Ensures that users always have at least one way to be contacted or identified.
/// </summary>
public record ContactInfo : RecordValueObject
{
    /// <summary>
    /// Phone number (optional, but at least one of Phone or Email must be provided)
    /// </summary>
    [MaxLength(15)]
    public string? PhoneNumber { get; init; }

    /// <summary>
    /// Email address (optional, but at least one of Phone or Email must be provided)
    /// </summary>
    [MaxLength(100)]
    public string? Email { get; init; }

    private ContactInfo()
    {
    }

    private ContactInfo(string? phoneNumber, string? email)
    {
        PhoneNumber = phoneNumber;
        Email = email;
    }

    /// <summary>
    /// Creates a ContactInfo instance with at least one contact method.
    /// </summary>
    /// <param name="phoneNumber">Phone number (10-15 digits)</param>
    /// <param name="email">Email address</param>
    /// <returns>A new ContactInfo instance</returns>
    /// <exception cref="ArgumentException">Thrown when both phone and email are empty, or format is invalid</exception>
    public static ContactInfo Create(string? phoneNumber, string? email)
    {
        // At least one contact method is required
        if (string.IsNullOrWhiteSpace(phoneNumber) && string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("At least one contact method (phone number or email) is required");

        // Validate phone number format if provided
        if (!string.IsNullOrWhiteSpace(phoneNumber) && !IsValidPhoneNumber(phoneNumber))
            throw new ArgumentException("Invalid phone number format.", nameof(phoneNumber));

        // Validate email format if provided
        if (!string.IsNullOrWhiteSpace(email) && !IsValidEmail(email))
            throw new ArgumentException("Invalid email format", nameof(email));

        return new ContactInfo(
            string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim(),
            string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant()
        );
    }

    /// <summary>
    /// Creates a ContactInfo with only phone number.
    /// </summary>
    public static ContactInfo FromPhoneNumber(string phoneNumber)
    {
        return Create(phoneNumber, null);
    }

    /// <summary>
    /// Creates a ContactInfo with only email.
    /// </summary>
    public static ContactInfo FromEmail(string email)
    {
        return Create(null, email);
    }

    /// <summary>
    /// Updates the phone number while ensuring at least one contact method remains.
    /// </summary>
    public ContactInfo UpdatePhoneNumber(string? newPhoneNumber)
    {
        if (string.IsNullOrWhiteSpace(newPhoneNumber) && string.IsNullOrWhiteSpace(Email))
            throw new InvalidOperationException("Cannot remove phone number when email is not set");

        return Create(newPhoneNumber, Email);
    }

    /// <summary>
    /// Updates the email while ensuring at least one contact method remains.
    /// </summary>
    public ContactInfo UpdateEmail(string? newEmail)
    {
        if (string.IsNullOrWhiteSpace(newEmail) && string.IsNullOrWhiteSpace(PhoneNumber))
            throw new InvalidOperationException("Cannot remove email when phone number is not set");

        return Create(PhoneNumber, newEmail);
    }

    /// <summary>
    /// Checks if phone number is set.
    /// </summary>
    public bool HasPhoneNumber => !string.IsNullOrWhiteSpace(PhoneNumber);

    /// <summary>
    /// Checks if email is set.
    /// </summary>
    public bool HasEmail => !string.IsNullOrWhiteSpace(Email);

    /// <summary>
    /// Gets the primary identifier (phone if available, otherwise email).
    /// </summary>
    public string? PrimaryIdentifier => PhoneNumber ?? Email;

    public override string ToString()
    {
        if (HasPhoneNumber && HasEmail)
            return $"Phone: {PhoneNumber}, Email: {Email}";
        if (HasPhoneNumber)
            return $"Phone: {PhoneNumber}";
        return $"Email: {Email}";
    }

    private static bool IsValidPhoneNumber(string phone)
    {
        return FormatChecker.IsValidPhoneNumber(phone);
    }

    private static bool IsValidEmail(string email)
    {
        return FormatChecker.IsValidEmail(email);
    }
}
