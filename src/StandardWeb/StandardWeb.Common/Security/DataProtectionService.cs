using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using MiCake.Core.DependencyInjection;

namespace StandardWeb.Common.Security;

/// <summary>
/// Provides encryption and decryption for sensitive data.
/// Uses ASP.NET Core Data Protection API for key management and encryption.
/// Can be used across multiple modules (Configuration, User, Auth, etc.).
/// </summary>
public interface IDataProtectionService
{
    /// <summary>
    /// Encrypts plain text for secure storage.
    /// </summary>
    /// <param name="plainText">Plain text to encrypt</param>
    /// <param name="purpose">Purpose string for key isolation (e.g., "AppSettings", "UserData")</param>
    /// <returns>Encrypted string</returns>
    /// <exception cref="ArgumentException">If plainText is null or empty</exception>
    string Protect(string plainText, string purpose = "AppData");

    /// <summary>
    /// Decrypts encrypted value back to plain text.
    /// </summary>
    /// <param name="encryptedText">Encrypted text to decrypt</param>
    /// <param name="purpose">Purpose string used during encryption</param>
    /// <returns>Decrypted plain text</returns>
    /// <exception cref="ArgumentException">If encryptedText is null or empty</exception>
    /// <exception cref="System.Security.Cryptography.CryptographicException">If decryption fails</exception>
    string Unprotect(string encryptedText, string purpose = "AppData");

    /// <summary>
    /// Checks if a string appears to be encrypted.
    /// Uses heuristic: Data Protection output is Base64-like and relatively long.
    /// </summary>
    /// <param name="value">Value to check</param>
    /// <returns>True if value appears encrypted</returns>
    bool IsEncrypted(string value);
}

/// <summary>
/// Implementation of IDataProtectionService using ASP.NET Core Data Protection API.
/// Registered as Singleton for better performance (stateless service).
/// </summary>
[InjectService(ServiceTypes = new[] { typeof(IDataProtectionService) },
               Lifetime = MiCakeServiceLifetime.Singleton)]
public class DataProtectionService : IDataProtectionService
{
    private readonly IDataProtectionProvider _protectionProvider;
    private readonly ILogger<DataProtectionService> _logger;

    public DataProtectionService(
        IDataProtectionProvider protectionProvider,
        ILogger<DataProtectionService> logger)
    {
        _protectionProvider = protectionProvider ?? throw new ArgumentNullException(nameof(protectionProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Protect(string plainText, string purpose = "AppData")
    {
        if (string.IsNullOrEmpty(plainText))
            throw new ArgumentException("Plain text cannot be null or empty", nameof(plainText));

        if (string.IsNullOrWhiteSpace(purpose))
            throw new ArgumentException("Purpose cannot be null or whitespace", nameof(purpose));

        try
        {
            var protector = _protectionProvider.CreateProtector(purpose);
            var encrypted = protector.Protect(plainText);

            _logger.LogDebug("Successfully encrypted data with purpose: {Purpose}", purpose);
            return encrypted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to encrypt data with purpose: {Purpose}", purpose);
            throw;
        }
    }

    public string Unprotect(string encryptedText, string purpose = "AppData")
    {
        if (string.IsNullOrEmpty(encryptedText))
            throw new ArgumentException("Encrypted text cannot be null or empty", nameof(encryptedText));

        if (string.IsNullOrWhiteSpace(purpose))
            throw new ArgumentException("Purpose cannot be null or whitespace", nameof(purpose));

        try
        {
            var protector = _protectionProvider.CreateProtector(purpose);
            var decrypted = protector.Unprotect(encryptedText);

            _logger.LogDebug("Successfully decrypted data with purpose: {Purpose}", purpose);
            return decrypted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decrypt data with purpose: {Purpose}", purpose);
            throw;
        }
    }

    public bool IsEncrypted(string value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        // Heuristic: Data Protection API output is Base64-encoded
        // Typical characteristics:
        // - Length > 50 characters (encrypted data is longer than plain text)
        // - No whitespace
        // - Contains only Base64 characters (A-Z, a-z, 0-9, +, /, =)
        return value.Length > 50 &&
               !value.Any(char.IsWhiteSpace) &&
               value.All(c => char.IsLetterOrDigit(c) || c == '+' || c == '/' || c == '=');
    }
}
