namespace StandardWeb.Common.Helpers;

using System.Security.Cryptography;
using System.Text;
using System.IO;

/// <summary>
/// Provides encryption and hashing utilities for password management and data security.
/// </summary>
public static class EncryptionHelper
{
    /// <summary>
    /// Hashes content using BCrypt algorithm with optional salt generation.
    /// BCrypt is a password hashing function designed to be slow and resistant to brute-force attacks.
    /// </summary>
    /// <param name="content">Content to hash (typically a password)</param>
    /// <param name="useSalt">Whether to generate a new salt (recommended for passwords)</param>
    /// <returns>Tuple containing the hash and optional salt value</returns>
    /// <exception cref="ArgumentNullException">Thrown when content is null or empty</exception>
    public static (string hash, string? salt) HashContent(string content, bool useSalt = true)
    {
        if (string.IsNullOrEmpty(content))
        {
            throw new ArgumentNullException(nameof(content), "Content cannot be null or empty.");
        }

        // Generate BCrypt hash with or without explicit salt
        var saltValue = useSalt ? BCrypt.Net.BCrypt.GenerateSalt() : null;
        var hash = saltValue == null ? BCrypt.Net.BCrypt.HashPassword(content) : BCrypt.Net.BCrypt.HashPassword(content, saltValue);
        return (hash, saltValue);
    }

    /// <summary>
    /// Verifies if content matches the BCrypt hash.
    /// </summary>
    /// <param name="content">Original content to verify</param>
    /// <param name="hash">BCrypt hash to verify against</param>
    /// <param name="salt">Salt value (unused in BCrypt verification but kept for API consistency)</param>
    /// <returns>True if content matches the hash, false otherwise</returns>
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

        // BCrypt.Verify handles salt extraction internally
        return BCrypt.Net.BCrypt.Verify(content, hash);
    }

    /// <summary>
    /// Partially masks a secret string for safe display in logs or UI.
    /// Shows first and last N characters, replacing middle with asterisks.
    /// </summary>
    /// <param name="secret">Secret string to mask</param>
    /// <param name="visibleChars">Number of characters to show at start and end</param>
    /// <returns>Masked string (e.g., "1234****5678" for visibleChars=4)</returns>
    public static string HideSecret(string secret, int visibleChars = 4)
    {
        if (string.IsNullOrEmpty(secret) || visibleChars < 0)
        {
            return string.Empty;
        }

        // If secret is too short, mask completely
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
    /// Encrypts plaintext using AES (Advanced Encryption Standard) algorithm.
    /// Uses a fixed zero IV (Initialization Vector) - suitable for non-critical data only.
    /// </summary>
    /// <param name="plainText">Text to encrypt</param>
    /// <param name="encryptionKey">AES encryption key (must be UTF-8 encoded to 16/24/32 bytes)</param>
    /// <returns>Base64-encoded encrypted data</returns>
    public static string AESEncrypt(string plainText, string encryptionKey)
    {
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(encryptionKey);
        aes.IV = new byte[16]; // Zero IV - consider using random IV for production

        using var encryptor = aes.CreateEncryptor();
        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
        using var sw = new StreamWriter(cs);
        sw.Write(plainText);
        sw.Close();
        return Convert.ToBase64String(ms.ToArray());
    }

    /// <summary>
    /// Decrypts AES-encrypted ciphertext back to plaintext.
    /// Must use the same key and IV as encryption.
    /// </summary>
    /// <param name="cipherText">Base64-encoded encrypted data</param>
    /// <param name="encryptionKey">AES decryption key (same as encryption key)</param>
    /// <returns>Decrypted plaintext</returns>
    public static string AESDecrypt(string cipherText, string encryptionKey)
    {
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(encryptionKey);
        aes.IV = new byte[16]; // Must match encryption IV

        using var decryptor = aes.CreateDecryptor();
        using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);
        return sr.ReadToEnd();
    }
}
