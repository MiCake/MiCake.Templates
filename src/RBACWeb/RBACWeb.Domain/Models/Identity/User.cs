using System.ComponentModel.DataAnnotations;
using RBACWeb.Common.Time;
using RBACWeb.Domain.Enums.Identity;

namespace RBACWeb.Domain.Models.Identity;

public class User : AuditAggregateRoot
{
    /// <summary>
    /// Contact information - at least one contact method (phone or email) is required
    /// </summary>
    [Required]
    public ContactInfo Contact { get; private set; } = null!;

    /// <summary>
    /// Password credentials - nullable to support external-login-only users
    /// </summary>
    public Password? Credential { get; private set; }

    /// <summary>
    /// Personal profile information
    /// </summary>
    public PersonalInfo Profile { get; private set; } = PersonalInfo.Empty;

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

    private readonly List<UserRole> _userRoles = [];
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    private readonly List<UserLoginHistory> _loginHistory = [];
    public IReadOnlyCollection<UserLoginHistory> LoginHistory => _loginHistory.AsReadOnly();

    #endregion

    private const int MaxLoginAttempts = 5;

    protected User() { }

    /// <summary>
    /// Registers a new user with phone number and password.
    /// </summary>
    public static User RegisterWithPhoneNumber(string phoneNumber, string passwordHash, string? salt = null)
    {
        var contact = ContactInfo.FromPhoneNumber(phoneNumber);
        var password = Password.Create(passwordHash, salt);

        return new User
        {
            Contact = contact,
            Credential = password,
            Profile = PersonalInfo.Empty
        };
    }

    /// <summary>
    /// Registers a new user with email and password.
    /// </summary>
    public static User RegisterWithEmail(string email, string passwordHash, string? salt = null)
    {
        var contact = ContactInfo.FromEmail(email);
        var password = Password.Create(passwordHash, salt);

        return new User
        {
            Contact = contact,
            Credential = password,
            Profile = PersonalInfo.Empty
        };
    }

    /// <summary>
    /// Registers a new user with both phone number and email.
    /// </summary>
    public static User RegisterWithBoth(string phoneNumber, string email, string passwordHash, string? salt = null)
    {
        var contact = ContactInfo.Create(phoneNumber, email);
        var password = Password.Create(passwordHash, salt);

        return new User
        {
            Contact = contact,
            Credential = password,
            Profile = PersonalInfo.Empty
        };
    }

    public void UpdateEmail(string? email)
    {
        Contact = Contact.UpdateEmail(email);
    }

    public void UpdatePhoneNumber(string? phoneNumber)
    {
        Contact = Contact.UpdatePhoneNumber(phoneNumber);
    }

    public void UpdateProfile(PersonalInfo info)
    {
        Profile = info ?? PersonalInfo.Empty;
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

        Credential = Password.Create(newPasswordHash, newSalt);
    }

    public bool HasPassword() => Credential is not null;

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
        var hasPasswordLogin = HasPassword();
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
            throw new ArgumentException("Provider key cannot be empty");

        var existing = _externalLogins.FirstOrDefault(e => e.ProviderType == externalLogin.ProviderType && e.ProviderKey == externalLogin.ProviderKey);

        if (existing is not null)
        {
            if (!existing.IsUnbound)
            {
                // Update existing active login
                existing.UpdateUserProfile(externalLogin.NickName, externalLogin.AvatarUrl);
                existing.RecordLogin();
                return true;
            }
            else
            {
                // Rebind unbound login
                existing.Rebind();
                existing.UpdateUserProfile(externalLogin.NickName, externalLogin.AvatarUrl);
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
        var hasPasswordLogin = HasPassword();
        var hasActiveExternalLogin = _externalLogins.Any(e => !e.IsUnbound);

        return hasPasswordLogin || hasActiveExternalLogin;
    }

    #endregion

    #region Login History Management

    /// <summary>
    /// Records a successful login attempt and resets failed count.
    /// </summary>
    public void RecordSuccessfulLogin(UserLoginHistory record)
    {
        ArgumentNullException.ThrowIfNull(record);

        if (!record.LoginSuccessful)
            throw new ArgumentException("Record must indicate a successful login", nameof(record));

        _loginHistory.Add(record);
        ResetAccessFailedCount();
    }

    /// <summary>
    /// Records a failed login attempt and increments failed count.
    /// </summary>
    public void RecordFailedLogin(UserLoginHistory record)
    {
        ArgumentNullException.ThrowIfNull(record);

        if (record.LoginSuccessful)
            throw new ArgumentException("Record must indicate a failed login", nameof(record));

        _loginHistory.Add(record);
        IncrementAccessFailedCount();
    }

    #endregion

    #region User Token Management

    public bool AddOrUpdateUserToken(UserToken userToken)
    {
        ArgumentNullException.ThrowIfNull(userToken);
        if (string.IsNullOrWhiteSpace(userToken.Value))
            throw new ArgumentException("Token value cannot be empty");

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

    #region Role Management

    /// <summary>
    /// Gets all effective (active and not expired) role IDs for this user.
    /// </summary>
    public IEnumerable<long> GetEffectiveRoleIds()
    {
        return _userRoles.Where(ur => ur.IsEffective()).Select(ur => ur.RoleId);
    }

    /// <summary>
    /// Checks if the user has a specific role.
    /// </summary>
    public bool HasRole(long roleId)
    {
        return _userRoles.Any(ur => ur.RoleId == roleId && ur.IsEffective());
    }

    /// <summary>
    /// Assigns a role to the user.
    /// </summary>
    public void AssignRole(long roleId, DateTime? expiresAt = null)
    {
        if (_userRoles.Any(ur => ur.RoleId == roleId && ur.IsEffective()))
            throw new InvalidOperationException($"Role {roleId} is already assigned to this user");

        var userRole = UserRole.Create(Id, roleId, expiresAt);
        userRole.SetUser(Id);
        _userRoles.Add(userRole);
    }

    /// <summary>
    /// Removes a role from the user.
    /// </summary>
    public void RemoveRole(long roleId)
    {
        var userRole = _userRoles.FirstOrDefault(ur => ur.RoleId == roleId && ur.IsEffective())
            ?? throw new InvalidOperationException($"Role {roleId} is not assigned to this user");

        userRole.Deactivate();
    }

    #endregion
}