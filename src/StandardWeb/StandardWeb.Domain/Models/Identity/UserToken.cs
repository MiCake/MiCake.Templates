using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using StandardWeb.Common.Time;
using StandardWeb.Domain.Enums.Identity;

namespace StandardWeb.Domain.Models.Identity;

/// <summary>
/// Represents a token associated with a user, such as for password reset, email verification, etc.
/// </summary>
public class UserToken : AuditEntity
{
    [Required]
    [MaxLength(100)]
    public UserTokenType Type { get; private set; }

    [Required]
    public string Value { get; private set; } = string.Empty;

    public DateTime? ExpiryDate { get; private set; }

    #region Navigation Properties
    public long UserId { get; private set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; private set; } = null!;
    #endregion

    // Required by EF Core
    protected UserToken() { }

    public static UserToken Create(UserTokenType type, string value, DateTime? expiryDate = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Token value cannot be empty", nameof(value));

        return new UserToken
        {
            Type = type,
            Value = value,
            ExpiryDate = expiryDate
        };
    }

    public void UpdateValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Token value cannot be empty", nameof(value));

        Value = value;
    }

    public void SetExpiry(DateTime expiry)
    {
        ExpiryDate = expiry;
    }

    public void ExtendExpiry(TimeSpan extension)
    {
        if (ExpiryDate.HasValue)
        {
            ExpiryDate = ExpiryDate.Value.Add(extension);
        }
        else
        {
            ExpiryDate = TimeNow.Now.Add(extension);
        }
    }

    public bool HasExpired()
    {
        return ExpiryDate.HasValue && ExpiryDate.Value < TimeNow.Now;
    }

    public void SetUser(User user)
    {
        UserId = user.Id;
    }
}