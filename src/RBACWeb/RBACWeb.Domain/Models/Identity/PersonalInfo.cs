using System.ComponentModel.DataAnnotations;
using RBACWeb.Common.Time;

namespace RBACWeb.Domain.Models.Identity;

/// <summary>
/// Personal information value object - encapsulates user profile data.
/// All fields are optional to support flexible user profiles.
/// </summary>
public record PersonalInfo : RecordValueObject
{
    /// <summary>
    /// User's first name
    /// </summary>
    [MaxLength(100)]
    public string? FirstName { get; init; }

    /// <summary>
    /// User's last name
    /// </summary>
    [MaxLength(100)]
    public string? LastName { get; init; }

    /// <summary>
    /// Display name shown in the UI
    /// </summary>
    [MaxLength(100)]
    public string? DisplayName { get; init; }

    /// <summary>
    /// Date of birth
    /// </summary>
    public DateTimeOffset? DateOfBirth { get; init; }

    private PersonalInfo()
    {
    }

    private PersonalInfo(string? firstName, string? lastName, string? displayName, DateTimeOffset? dateOfBirth)
    {
        FirstName = firstName;
        LastName = lastName;
        DisplayName = displayName;
        DateOfBirth = dateOfBirth;
    }

    /// <summary>
    /// Creates a PersonalInfo instance.
    /// </summary>
    public static PersonalInfo Create(string? firstName = null, string? lastName = null, string? displayName = null, DateTimeOffset? dateOfBirth = null)
    {
        // Validate date of birth if provided
        if (dateOfBirth.HasValue)
        {
            if (dateOfBirth.Value > TimeNow.Now)
                throw new ArgumentException("Date of birth cannot be in the future", nameof(dateOfBirth));
        }

        return new PersonalInfo(
            string.IsNullOrWhiteSpace(firstName) ? null : firstName.Trim(),
            string.IsNullOrWhiteSpace(lastName) ? null : lastName.Trim(),
            string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim(),
            dateOfBirth
        );
    }

    /// <summary>
    /// Empty personal info (all fields null)
    /// </summary>
    public static PersonalInfo Empty => new(null, null, null, null);

    /// <summary>
    /// Gets the full name (FirstName + LastName), or DisplayName if names are not set.
    /// </summary>
    public string FullName
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(FirstName) || !string.IsNullOrWhiteSpace(LastName))
                return $"{FirstName} {LastName}".Trim();
            
            return DisplayName ?? "Unknown";
        }
    }

    /// <summary>
    /// Gets the calculated age based on date of birth, or null if not set.
    /// </summary>
    public int? Age
    {
        get
        {
            if (!DateOfBirth.HasValue)
                return null;

            var today = TimeNow.Now.Date;
            var age = today.Year - DateOfBirth.Value.Year;

            // Adjust if birthday hasn't occurred this year yet
            if (DateOfBirth.Value.Date > today.AddYears(-age))
                age--;

            return age;
        }
    }

    /// <summary>
    /// Checks if any personal information is provided.
    /// </summary>
    public bool IsEmpty => string.IsNullOrWhiteSpace(FirstName) &&
                           string.IsNullOrWhiteSpace(LastName) &&
                           string.IsNullOrWhiteSpace(DisplayName) &&
                           !DateOfBirth.HasValue;

    public override string ToString()
    {
        return FullName;
    }
}
