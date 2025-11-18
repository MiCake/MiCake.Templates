using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using StandardWeb.Common.Time;
using StandardWeb.Domain.Enums.Identity;

namespace StandardWeb.Domain.Models.Identity;

public class UserLoginHistory : AuditAggregateRoot
{
    [Required]
    public DateTime RecordedAt { get; private set; }

    [Required]
    public UserLoginActionType ActionType { get; private set; }

    [MaxLength(45)]
    public string? IpAddress { get; private set; }

    [MaxLength(256)]
    public string? UserAgent { get; private set; }

    [MaxLength(100)]
    public string? Device { get; private set; }

    [MaxLength(100)]
    public string? Location { get; private set; }

    public bool LoginSuccessful { get; private set; }

    [MaxLength(256)]
    public string? FailureReason { get; private set; }

    #region Navigation Properties
    public long UserId { get; private set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; private set; } = null!;
    #endregion

    // Required by EF Core
    protected UserLoginHistory() { }

    public static UserLoginHistory CreateSuccessfulLogin(
        long userId,
        UserLoginActionType action,
        string ipAddress,
        string userAgent,
        string device = "",
        string location = "")
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            throw new ArgumentException("IP address cannot be empty", nameof(ipAddress));

        if (string.IsNullOrWhiteSpace(userAgent))
            throw new ArgumentException("User agent cannot be empty", nameof(userAgent));

        return new UserLoginHistory
        {
            UserId = userId,
            RecordedAt = TimeNow.Now,
            ActionType = action,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            LoginSuccessful = true,
            Device = device,
            Location = location
        };
    }

    public static UserLoginHistory CreateFailedLogin(
        long userId,
        UserLoginActionType action,
        string ipAddress,
        string userAgent,
        string failureReason,
        string device = "",
        string location = "")
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            throw new ArgumentException("IP address cannot be empty", nameof(ipAddress));

        if (string.IsNullOrWhiteSpace(userAgent))
            throw new ArgumentException("User agent cannot be empty", nameof(userAgent));

        return new UserLoginHistory
        {
            UserId = userId,
            RecordedAt = TimeNow.Now,
            ActionType = action,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            LoginSuccessful = false,
            FailureReason = failureReason,
            Device = device,
            Location = location
        };
    }
}