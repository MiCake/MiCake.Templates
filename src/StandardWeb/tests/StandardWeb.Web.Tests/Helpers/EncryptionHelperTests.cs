using StandardWeb.Common.Helpers;

namespace StandardWeb.Web.Tests.Helpers;

/// <summary>
/// Unit tests for EncryptionHelper to validate password hashing and encryption.
/// </summary>
public class EncryptionHelperTests
{
    [Fact]
    public void HashContent_ShouldGenerateValidHash()
    {
        // Arrange
        var content = "password123";

        // Act
        var (hash, salt) = EncryptionHelper.HashContent(content);

        // Assert
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
        Assert.NotNull(salt);
    }

    [Fact]
    public void VerifyHash_WithCorrectContent_ShouldReturnTrue()
    {
        // Arrange
        var content = "password123";
        var (hash, salt) = EncryptionHelper.HashContent(content);

        // Act
        var result = EncryptionHelper.VerifyHash(content, hash, salt ?? "");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyHash_WithIncorrectContent_ShouldReturnFalse()
    {
        // Arrange
        var correctContent = "password123";
        var wrongContent = "wrongpassword";
        var (hash, salt) = EncryptionHelper.HashContent(correctContent);

        // Act
        var result = EncryptionHelper.VerifyHash(wrongContent, hash, salt ?? "");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HideSecret_ShouldMaskMiddlePortion()
    {
        // Arrange
        var secret = "1234567890";
        var visibleChars = 2;

        // Act
        var result = EncryptionHelper.HideSecret(secret, visibleChars);

        // Assert
        Assert.StartsWith("12", result);
        Assert.EndsWith("90", result);
        Assert.Contains("*", result);
        Assert.Equal(secret.Length, result.Length);
    }

    [Fact]
    public void AESEncrypt_AndDecrypt_ShouldRoundTrip()
    {
        // Arrange
        var plainText = "Sensitive Data";
        var key = "1234567890123456"; // 16 characters for AES

        // Act
        var encrypted = EncryptionHelper.AESEncrypt(plainText, key);
        var decrypted = EncryptionHelper.AESDecrypt(encrypted, key);

        // Assert
        Assert.NotEqual(plainText, encrypted);
        Assert.Equal(plainText, decrypted);
    }
}

