namespace StandardWeb.Common.Helpers;

using System.Security.Cryptography;
using System.Text;
using System.IO;

/// <summary>
/// Provides encryption and hashing utilities for password management and data protection
/// </summary>
public static class EncryptionHelper
{
    /// <summary>
    /// Hashes content using BCrypt algorithm with optional salt generation
    /// </summary>
    /// <param name="content">The content to hash (typically a password)</param>
    /// <param name="useSalt">Whether to generate and use a salt value</param>
    /// <returns>A tuple containing the hash and optional salt value</returns>
    /// <exception cref="ArgumentNullException">Thrown when content is null or empty</exception>
    public static (string hash, string? salt) HashContent(string content, bool useSalt = true)
    {
        if (string.IsNullOrEmpty(content))
        {
            throw new ArgumentNullException(nameof(content), "Content cannot be null or empty.");
        }

        // Generate salt and hash using BCrypt
        var saltValue = useSalt ? BCrypt.Net.BCrypt.GenerateSalt() : null;
        var hash = saltValue == null ? BCrypt.Net.BCrypt.HashPassword(content) : BCrypt.Net.BCrypt.HashPassword(content, saltValue);
        return (hash, saltValue);
    }

    /// <summary>
    /// Verifies that the provided content matches the BCrypt hash
    /// </summary>
    /// <param name="content">The content to verify (typically a password)</param>
    /// <param name="hash">The BCrypt hash to verify against</param>
    /// <param name="salt">The salt value (not used in BCrypt verification but kept for API compatibility)</param>
    /// <returns>True if the content matches the hash; otherwise false</returns>
    /// <exception cref="ArgumentNullException">Thrown when content or hash is null or empty</exception>
    public static bool VerifyHash(string content, string hash, string salt = "")
    {
        if (string.IsNullOrEmpty(content))
        {
            throw new ArgumentNullException(nameof(content), "Content cannot be null or empty.");
        }

        if (string.IsNullOrEmpty(hash))
        {
            throw new ArgumentNullException(nameof(hash), "Hash cannot be null or empty.");
        }

        // Verify the hash using BCrypt
        return BCrypt.Net.BCrypt.Verify(content, hash);
    }

    /// <summary>
    /// Partially hides a secret string by masking the middle characters
    /// </summary>
    /// <param name="secret">The secret string to hide</param>
    /// <param name="visibleChars">Number of characters to show at the start and end</param>
    /// <returns>A string with the middle portion replaced by asterisks</returns>
    public static string HideSecret(string secret, int visibleChars = 4)
    {
        if (string.IsNullOrEmpty(secret) || visibleChars < 0)
        {
            return string.Empty;
        }

        if (secret.Length <= visibleChars * 2)
        {
            return new string('*', secret.Length);
        }

        var start = secret[..visibleChars];
        var end = secret[^visibleChars..];
        var hiddenPart = new string('*', secret.Length - visibleChars * 2);
        return $"{start}{hiddenPart}{end}";
    }

    /// <summary>
    /// Encrypts plaintext using AES encryption with the specified key
    /// </summary>
    /// <param name="plainText">The text to encrypt</param>
    /// <param name="encryptionKey">The AES encryption key (must be 16, 24, or 32 bytes when UTF-8 encoded)</param>
    /// <returns>A base64-encoded encrypted string</returns>
    public static string AESEncrypt(string plainText, string encryptionKey)
    {
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(encryptionKey);
        aes.IV = new byte[16]; // Zero IV - consider using a random IV for production

        using var encryptor = aes.CreateEncryptor();
        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
        using var sw = new StreamWriter(cs);
        sw.Write(plainText);
        sw.Close();
        return Convert.ToBase64String(ms.ToArray());
    }

    /// <summary>
    /// Decrypts an AES-encrypted string using the specified key
    /// </summary>
    /// <param name="cipherText">The base64-encoded encrypted text to decrypt</param>
    /// <param name="encryptionKey">The AES encryption key (must match the key used for encryption)</param>
    /// <returns>The decrypted plaintext string</returns>
    public static string AESDecrypt(string cipherText, string encryptionKey)
    {
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(encryptionKey);
        aes.IV = new byte[16]; // Must match the IV used during encryption

        using var decryptor = aes.CreateDecryptor();
        using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);
        return sr.ReadToEnd();
    }
}
