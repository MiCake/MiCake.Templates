using System.ComponentModel.DataAnnotations;
using StandardWeb.Common.Time;
using StandardWeb.Domain.Enums.Identity;

namespace StandardWeb.Domain.Models.Identity;

public class User : AuditAggregateRoot
{
    [MaxLength(15)]
    [Required]
    public string PhoneNumber { get; private set; } = null!;

    [MaxLength(100)]
    public string? Email { get; private set; }

    [Required]
    [MaxLength(300)]
    public string PasswordHash { get; private set; } = string.Empty;

    [MaxLength(50)]
    public string? Salt { get; private set; }

    [MaxLength(100)]
    public string? FirstName { get; private set; }

    [MaxLength(100)]
    public string? LastName { get; private set; }

    [MaxLength(100)]
    public string? DisplayName { get; private set; }

    public DateTime? DateOfBirth { get; private set; }

    public string? ProfilePictureUrl { get; private set; }

    public DateTime? LockoutEnd { get; private set; }

    public bool LockoutEnabled { get; private set; } = false;

    public int AccessFailedCount { get; private set; } = 0;

    public UserStatus Status { get; private set; } = UserStatus.Active;

    public bool ForceOTPOnLogin { get; private set; } = false;

    #region Navigation Properties

    private readonly List<ExternalLoginProvider> _externalLogins = [];
    public IReadOnlyCollection<ExternalLoginProvider> ExternalLogins => _externalLogins.AsReadOnly();

    private readonly List<UserToken> _userTokens = [];
    public IReadOnlyCollection<UserToken> UserTokens => _userTokens.AsReadOnly();

    #endregion

    private const int MaxLoginAttempts = 5;

    protected User() { }

    public static User RegisterNewUser(string phoneNumber, string passwordHash, string? salt = null)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number cannot be empty", nameof(phoneNumber));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty", nameof(passwordHash));

        var user = new User
        {
            PhoneNumber = phoneNumber,
            PasswordHash = passwordHash,
            Salt = salt,
        };

        return user;
    }

    public void UpdateProfile(string? firstName, string? lastName, string? displayName, DateTime? dateOfBirth = null)
    {
        FirstName = firstName ?? string.Empty;
        LastName = lastName ?? string.Empty;
        DisplayName = displayName ?? string.Empty;

        if (dateOfBirth.HasValue)
        {
            DateOfBirth = dateOfBirth;
        }
    }

    public void UpdateEmail(string? email)
    {
        Email = email;
    }

    public void SetProfilePicture(string pictureUrl)
    {
        if (string.IsNullOrWhiteSpace(pictureUrl))
            throw new ArgumentException("Picture URL cannot be empty", nameof(pictureUrl));

        ProfilePictureUrl = pictureUrl;
    }

    public void LockAccount(TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero)
            throw new ArgumentException("Lock duration must be positive", nameof(duration));

        LockoutEnabled = true;
        LockoutEnd = TimeNow.Now.Add(duration);
    }

    public void UnlockAccount()
    {
        LockoutEnabled = false;
        LockoutEnd = null;
        AccessFailedCount = 0;
    }

    public bool IsLockedOut()
    {
        return LockoutEnabled && LockoutEnd.HasValue && LockoutEnd.Value > TimeNow.Now;
    }

    public void IncrementAccessFailedCount()
    {
        AccessFailedCount++;
        if (AccessFailedCount >= MaxLoginAttempts)
        {
            MarkDangerousLogin();
        }
    }

    public void ResetAccessFailedCount()
    {
        AccessFailedCount = 0;
        MarkSafeLogin();
    }

    public void UpdateStatus(UserStatus status)
    {
        Status = status;
    }

    public void ChangePassword(string newPasswordHash, string? newSalt)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("Password hash cannot be empty", nameof(newPasswordHash));

        PasswordHash = newPasswordHash;
        Salt = newSalt;
    }

    public void MarkDangerousLogin()
    {
        ForceOTPOnLogin = true;
    }

    public void MarkSafeLogin()
    {
        ForceOTPOnLogin = false;
    }

    #region External Login Management

    public IReadOnlyList<ExternalLoginProvider> GetActiveExternalLogins()
    {
        return _externalLogins.Where(e => !e.IsUnbound).ToList().AsReadOnly();
    }

    public void RemoveExternalLogin(LoginProviderType providerType)
    {
        var externalLogin = _externalLogins.FirstOrDefault(e => e.ProviderType == providerType && !e.IsUnbound)
            ?? throw new InvalidOperationException($"External login {providerType} not found or already unbound");

        // Safety check: prevent removing the only login method
        var hasPasswordLogin = !string.IsNullOrWhiteSpace(PasswordHash);
        var otherActiveLogins = _externalLogins.Count(e => e.ProviderType != providerType && !e.IsUnbound);

        if (!hasPasswordLogin && otherActiveLogins == 0)
        {
            throw new InvalidOperationException(
                "Cannot unbind the only login method. Please set a password or bind another login method first.");
        }

        externalLogin.Unbind();
    }

    public bool AddOrUpdateExternalLogin(ExternalLoginProvider externalLogin)
    {
        ArgumentNullException.ThrowIfNull(externalLogin);
        if (string.IsNullOrWhiteSpace(externalLogin.ProviderKey) || externalLogin.ProviderType == default)
            throw new ArgumentException("Provider key cannot be empty", nameof(externalLogin.ProviderKey));

        var existing = _externalLogins.FirstOrDefault(e => e.ProviderType == externalLogin.ProviderType && e.ProviderKey == externalLogin.ProviderKey);

        if (existing is not null)
        {
            if (!existing.IsUnbound)
            {
                // Update existing active login
                existing.UpdateUserProfile(externalLogin.NickName, externalLogin.AvatarUrl);
                existing.UpdateTokens(externalLogin.AccessToken);
                existing.RecordLogin();
                return true;
            }
            else
            {
                // Rebind unbound login
                existing.Rebind();
                existing.UpdateUserProfile(externalLogin.NickName, externalLogin.AvatarUrl);
                existing.UpdateTokens(externalLogin.AccessToken);
                return true;
            }
        }
        else
        {
            externalLogin.SetUser(this);
            _externalLogins.Add(externalLogin);
            return true;
        }
    }

    public bool HasAnyLoginMethod()
    {
        var hasPasswordLogin = !string.IsNullOrWhiteSpace(PasswordHash);
        var hasActiveExternalLogin = _externalLogins.Any(e => !e.IsUnbound);

        return hasPasswordLogin || hasActiveExternalLogin;
    }

    #endregion

    #region User Token Management

    public bool AddOrUpdateUserToken(UserToken userToken)
    {
        ArgumentNullException.ThrowIfNull(userToken);
        if (string.IsNullOrWhiteSpace(userToken.Value))
            throw new ArgumentException("Token value cannot be empty", nameof(userToken.Value));

        var existing = _userTokens.FirstOrDefault(t => t.Type == userToken.Type && !t.HasExpired());

        if (existing is not null)
        {
            existing.UpdateValue(userToken.Value);
            existing.SetExpiry(userToken.ExpiryDate ?? TimeNow.Now.AddHours(24)); // Default 24 hours if not specified
            return true;
        }
        else
        {
            userToken.SetUser(this);
            _userTokens.Add(userToken);
            return true;
        }
    }

    public ExternalLoginProvider? GetExternalLogin(LoginProviderType providerType)
    {
        return _externalLogins.FirstOrDefault(e => e.ProviderType == providerType && !e.IsUnbound);
    }

    public UserToken? GetUserToken(UserTokenType type)
    {
        return _userTokens.FirstOrDefault(t => t.Type == type);
    }

    #endregion
}