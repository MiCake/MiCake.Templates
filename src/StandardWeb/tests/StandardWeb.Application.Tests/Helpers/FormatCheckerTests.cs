using StandardWeb.Common.Helpers;

namespace StandardWeb.Application.Tests.Helpers;

/// <summary>
/// Unit tests for FormatChecker helper methods.
/// </summary>
public class FormatCheckerTests
{
    [Theory]
    [InlineData("13800138000", true)]
    [InlineData("13900139000", true)]
    [InlineData("18612345678", true)]
    [InlineData("12345678901", false)]  // Invalid prefix
    [InlineData("1380013800", false)]   // Too short
    [InlineData("138001380001", false)] // Too long
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidPhoneNumber_ShouldValidateCorrectly(string phoneNumber, bool expected)
    {
        // Act
        var result = FormatChecker.IsValidPhoneNumber(phoneNumber);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("test@example.com", true)]
    [InlineData("user.name@domain.co.uk", true)]
    [InlineData("invalid.email", false)]
    [InlineData("@example.com", false)]
    [InlineData("", false)]
    public void IsValidEmail_ShouldValidateCorrectly(string email, bool expected)
    {
        // Act
        var result = FormatChecker.IsValidEmail(email);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("user123", true)]
    [InlineData("test_user", true)]
    [InlineData("ab", false)]          // Too short
    [InlineData("verylongusername", false)] // Too long
    [InlineData("user name", false)]   // Contains space
    [InlineData("", false)]
    public void IsValidUsername_ShouldValidateCorrectly(string username, bool expected)
    {
        // Act
        var result = FormatChecker.IsValidUsername(username);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("pass1", true)]
    [InlineData("password123", true)]
    [InlineData("abc1", false)]        // Too short
    [InlineData("password", false)]    // No digit
    [InlineData("12345", false)]       // No letter
    [InlineData("", false)]
    public void IsValidPassword_ShouldValidateCorrectly(string password, bool expected)
    {
        // Act
        var result = FormatChecker.IsValidPassword(password);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Hello世界", true)]
    [InlineData("你好", true)]
    [InlineData("Hello World", false)]
    [InlineData("123456", false)]
    [InlineData("", false)]
    public void ContainsChinese_ShouldDetectCorrectly(string text, bool expected)
    {
        // Act
        var result = FormatChecker.ContainsChinese(text);

        // Assert
        Assert.Equal(expected, result);
    }
}

