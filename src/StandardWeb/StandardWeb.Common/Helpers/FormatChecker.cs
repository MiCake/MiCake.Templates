namespace StandardWeb.Common.Helpers;

/// <summary>
/// Provides validation methods for common input formats.
/// </summary>
public static class FormatChecker
{
    /// <summary>
    /// Validates email address format using System.Net.Mail.MailAddress.
    /// </summary>
    /// <param name="email">Email address to validate</param>
    /// <returns>True if email format is valid, false otherwise</returns>
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
    /// Validates Chinese mobile phone number format.
    /// Must be 11 digits starting with valid Chinese mobile prefixes (13-19).
    /// </summary>
    /// <param name="phoneNumber">Phone number to validate</param>
    /// <returns>True if phone number format is valid, false otherwise</returns>
    public static bool IsValidPhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        // Must be exactly 11 digits
        if (phoneNumber.Length != 11 || !long.TryParse(phoneNumber, out _))
            return false;

        // Must start with valid Chinese mobile prefix
        var validPrefixes = new[] { "13", "14", "15", "16", "17", "18", "19" };
        return validPrefixes.Any(prefix => phoneNumber.StartsWith(prefix));
    }

    /// <summary>
    /// Validates username format: 3-12 characters, alphanumeric with underscores/hyphens.
    /// </summary>
    /// <param name="username">Username to validate</param>
    /// <returns>True if username format is valid, false otherwise</returns>
    public static bool IsValidUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return false;

        // Username: 3-12 chars, letters/numbers/underscore/hyphen only
        return username.Length >= 3 && username.Length <= 12 && System.Text.RegularExpressions.Regex.IsMatch(username, @"^[a-zA-Z0-9_-]+$");
    }

    /// <summary>
    /// Validates password strength: minimum 5 characters with at least one letter and one digit.
    /// </summary>
    /// <param name="password">Password to validate</param>
    /// <returns>True if password meets minimum requirements, false otherwise</returns>
    public static bool IsValidPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        // Minimum 5 chars, must contain at least one letter and one digit
        return password.Length >= 5 &&
               System.Text.RegularExpressions.Regex.IsMatch(password, @"[a-zA-Z]") &&
               System.Text.RegularExpressions.Regex.IsMatch(password, @"\d");
    }

    /// <summary>
    /// Checks if text contains Chinese characters (Unicode range U+4E00 to U+9FFF).
    /// </summary>
    /// <param name="text">Text to check</param>
    /// <returns>True if text contains Chinese characters, false otherwise</returns>
    public static bool ContainsChinese(string text)
    {
        return !string.IsNullOrEmpty(text) && System.Text.RegularExpressions.Regex.IsMatch(text, "[\u4e00-\u9fff]");
    }
}
