namespace StandardWeb.Common.Helpers;

public static class FormatChecker
{
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

    public static bool IsValidPhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        // check if it is the china phone number format
        if (phoneNumber.Length != 11 || !long.TryParse(phoneNumber, out _))
            return false;

        // Check if it starts with a valid Chinese mobile prefix
        var validPrefixes = new[] { "13", "14", "15", "16", "17", "18", "19" };
        return validPrefixes.Any(prefix => phoneNumber.StartsWith(prefix));
    }

    public static bool IsValidUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return false;

        // Username must be between 3 and 12 characters long and can contain letters, numbers, underscores, and hyphens
        return username.Length >= 3 && username.Length <= 12 && System.Text.RegularExpressions.Regex.IsMatch(username, @"^[a-zA-Z0-9_-]+$");
    }

    public static bool IsValidPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        // Password must be at least 5 characters long and contain at least one letter and one number
        return password.Length >= 5 &&
               System.Text.RegularExpressions.Regex.IsMatch(password, @"[a-zA-Z]") &&
               System.Text.RegularExpressions.Regex.IsMatch(password, @"\d");
    }

    public static bool ContainsChinese(string text)
    {
        return !string.IsNullOrEmpty(text) && System.Text.RegularExpressions.Regex.IsMatch(text, "[\u4e00-\u9fff]");
    }
}
