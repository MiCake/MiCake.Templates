using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using StandardWeb.Common.Time;
using StandardWeb.Domain.Enums.Identity;

namespace StandardWeb.Domain.Models.Identity;

/// <summary>
/// External login provider entity for third-party authentication (WeChat, Alipay, Apple, etc.)
/// </summary>
public class ExternalLoginProvider : AuditEntity
{
    /// <summary>
    /// Login provider type (WeChat, Alipay, Apple, etc.)
    /// </summary>
    public LoginProviderType ProviderType { get; private set; }

    /// <summary>
    /// Provider's unique user identifier (e.g., WeChat OpenId, Alipay UserId)
    /// </summary>
    [MaxLength(200)]
    [Required]
    public string ProviderKey { get; private set; } = null!;

    /// <summary>
    /// Provider's global user identifier (e.g., WeChat UnionId, optional)
    /// </summary>
    [MaxLength(200)]
    public string? ProviderUnionId { get; private set; }

    /// <summary>
    /// User's nickname from third-party platform
    /// </summary>
    [MaxLength(100)]
    public string? NickName { get; private set; }

    /// <summary>
    /// User's avatar URL from third-party platform
    /// </summary>
    [MaxLength(500)]
    public string? AvatarUrl { get; private set; }

    /// <summary>
    /// Extended data from provider (JSON format)
    /// e.g., gender, city, language, etc.
    /// </summary>
    [MaxLength(2000)]
    public string? ExtendedData { get; private set; }

    /// <summary>
    /// Access token (optional, for subsequent API calls)
    /// </summary>
    [MaxLength(1000)]
    public string? AccessToken { get; private set; }

    /// <summary>
    /// Refresh token (optional)
    /// </summary>
    [MaxLength(1000)]
    public string? RefreshToken { get; private set; }

    /// <summary>
    /// Token expiration time (optional)
    /// </summary>
    public DateTime? TokenExpiresAt { get; private set; }

    /// <summary>
    /// First bind time
    /// </summary>
    public DateTime BindTime { get; private set; }

    /// <summary>
    /// Last login time
    /// </summary>
    public DateTime? LastLoginTime { get; private set; }

    /// <summary>
    /// Whether the account is unbound
    /// </summary>
    public bool IsUnbound { get; private set; }

    /// <summary>
    /// Unbind time
    /// </summary>
    public DateTime? UnboundTime { get; private set; }

    #region Navigation Properties
    public long UserId { get; private set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; private set; } = null!;
    #endregion

    protected ExternalLoginProvider() { }

    public static ExternalLoginProvider Create(
        LoginProviderType providerType,
        string providerKey,
        string? providerUnionId = null,
        string? nickName = null,
        string? avatarUrl = null,
        string? accessToken = null)
    {
        if (string.IsNullOrWhiteSpace(providerKey))
            throw new ArgumentException("Provider key cannot be empty", nameof(providerKey));

        return new ExternalLoginProvider
        {
            ProviderType = providerType,
            ProviderKey = providerKey,
            ProviderUnionId = providerUnionId,
            NickName = nickName,
            AvatarUrl = avatarUrl,
            AccessToken = accessToken,
            BindTime = TimeNow.Now,
            IsUnbound = false
        };
    }

    /// <summary>
    /// Update user profile information (nickname, avatar, etc.)
    /// </summary>
    public void UpdateUserProfile(string? nickName, string? avatarUrl, string? extendedData = null)
    {
        NickName = nickName;
        AvatarUrl = avatarUrl;

        if (!string.IsNullOrWhiteSpace(extendedData))
        {
            ExtendedData = extendedData;
        }
    }

    /// <summary>
    /// Update access tokens
    /// </summary>
    public void UpdateTokens(string? accessToken, string? refreshToken = null, DateTime? expiresAt = null)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        TokenExpiresAt = expiresAt;
    }

    /// <summary>
    /// Record login activity
    /// </summary>
    public void RecordLogin()
    {
        LastLoginTime = TimeNow.Now;
    }

    /// <summary>
    /// Unbind the external account
    /// </summary>
    public void Unbind()
    {
        if (IsUnbound)
            throw new InvalidOperationException("Already unbound");

        IsUnbound = true;
        UnboundTime = TimeNow.Now;

        // Clear sensitive information
        AccessToken = null;
        RefreshToken = null;
        TokenExpiresAt = null;
    }

    /// <summary>
    /// Rebind (can rebind after unbinding)
    /// </summary>
    public void Rebind()
    {
        if (!IsUnbound)
            throw new InvalidOperationException("Not in unbound state");

        IsUnbound = false;
        BindTime = TimeNow.Now;
        LastLoginTime = TimeNow.Now;
        UnboundTime = null;
    }

    public void SetUser(User user)
    {
        UserId = user.Id;
    }
}
