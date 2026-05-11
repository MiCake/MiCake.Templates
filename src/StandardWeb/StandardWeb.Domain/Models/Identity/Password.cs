using System.ComponentModel.DataAnnotations;

namespace StandardWeb.Domain.Models.Identity;

/// <summary>
/// Password value object - encapsulates password hash and salt.
/// Supports nullable passwords for users who only use external login providers.
/// </summary>
public record Password : RecordValueObject
{
    /// <summary>
    /// BCrypt password hash
    /// </summary>
    [MaxLength(300)]
    public string Hash { get; init; }

    /// <summary>
    /// Optional salt used in hashing (implementation-dependent)
    /// </summary>
    [MaxLength(100)]
    public string? Salt { get; init; }

    private Password()
    {
        Hash = string.Empty;
        Salt = null;
    }

    private Password(string hash, string? salt)
    {
        Hash = hash;
        Salt = salt;
    }

    /// <summary>
    /// Creates a Password instance with the given hash and optional salt.
    /// </summary>
    /// <param name="hash">Password hash (required)</param>
    /// <param name="salt">Optional salt</param>
    /// <returns>A new Password instance</returns>
    /// <exception cref="ArgumentException">Thrown when hash is empty</exception>
    public static Password Create(string hash, string? salt = null)
    {
        if (string.IsNullOrWhiteSpace(hash))
            throw new ArgumentException("Password hash cannot be empty", nameof(hash));

        return new Password(hash, salt);
    }

    /// <summary>
    /// Creates a new Password instance with updated hash and salt.
    /// </summary>
    public Password Update(string newHash, string? newSalt = null)
    {
        return Create(newHash, newSalt);
    }

    public override string ToString()
    {
        // Never expose password hash in string representation
        return "[Password Hash Protected]";
    }
}
