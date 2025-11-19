namespace StandardWeb.Common.Helpers;

/// <summary>
/// Provides validation methods for common data formats
/// </summary>
public static class FormatChecker
{
    /// <summary>
    /// Validates if the provided email address has a valid format
    /// </summary>
    /// <param name="email">The email address to validate</param>
    /// <returns>True if the email format is valid; otherwise false</returns>
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates if the provided phone number matches Chinese mobile phone format
    /// </summary>
    /// <param name="phoneNumber">The phone number to validate</param>
    /// <returns>True if the phone number is a valid Chinese mobile number; otherwise false</returns>
    public static bool IsValidPhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        // Validate Chinese mobile phone number format: 11 digits starting with valid prefix
        if (phoneNumber.Length != 11 || !long.TryParse(phoneNumber, out _))
            return false;

        // Check if it starts with a valid Chinese mobile prefix
        var validPrefixes = new[] { "13", "14", "15", "16", "17", "18", "19" };
        return validPrefixes.Any(prefix => phoneNumber.StartsWith(prefix));
    }

    /// <summary>
    /// Validates if the provided username meets the required format
    /// </summary>
    /// <param name="username">The username to validate</param>
    /// <returns>True if the username is valid (3-12 characters, alphanumeric, underscore, or hyphen); otherwise false</returns>
    public static bool IsValidUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return false;

        // Username must be 3-12 characters long and contain only letters, numbers, underscores, and hyphens
        return username.Length >= 3 && username.Length <= 12 && System.Text.RegularExpressions.Regex.IsMatch(username, @"^[a-zA-Z0-9_-]+$");
    }

    /// <summary>
    /// Validates if the provided password meets minimum security requirements
    /// </summary>
    /// <param name="password">The password to validate</param>
    /// <returns>True if the password is valid (at least 5 characters with letters and numbers); otherwise false</returns>
    public static bool IsValidPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        // Password must be at least 5 characters and contain both letters and numbers
        return password.Length >= 5 &&
               System.Text.RegularExpressions.Regex.IsMatch(password, @"[a-zA-Z]") &&
               System.Text.RegularExpressions.Regex.IsMatch(password, @"\d");
    }

    /// <summary>
    /// Checks if the provided text contains any Chinese characters
    /// </summary>
    /// <param name="text">The text to check</param>
    /// <returns>True if the text contains Chinese characters; otherwise false</returns>
    public static bool ContainsChinese(string text)
    {
        return !string.IsNullOrEmpty(text) && System.Text.RegularExpressions.Regex.IsMatch(text, "[\u4e00-\u9fff]");
    }
}
