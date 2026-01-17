using RBACWeb.Common.Time;
using RBACWeb.Domain.Models.Authorization;

namespace RBACWeb.Domain.Models.Identity;

/// <summary>
/// Represents the association between a User and a Role.
/// This is a child entity of the User aggregate.
/// </summary>
public class UserRole : AuditEntity
{
    /// <summary>
    /// The user that has this role.
    /// </summary>
    public long UserId { get; private set; }

    /// <summary>
    /// The role assigned to the user.
    /// </summary>
    public long RoleId { get; private set; }

    /// <summary>
    /// When the role was assigned to the user.
    /// </summary>
    public DateTime AssignedAt { get; private set; }

    /// <summary>
    /// Optional expiration date for temporary role assignments.
    /// </summary>
    public DateTime? ExpiresAt { get; private set; }

    /// <summary>
    /// Whether the role assignment is active.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    #region Navigation Properties

    /// <summary>
    /// The user that has this role.
    /// </summary>
    public User User { get; private set; } = null!;

    /// <summary>
    /// The role assigned to the user.
    /// </summary>
    public Role Role { get; private set; } = null!;

    #endregion

    protected UserRole() { }

    /// <summary>
    /// Creates a new user-role assignment.
    /// </summary>
    public static UserRole Create(long userId, long roleId, DateTime? expiresAt = null)
    {
        if (userId <= 0)
            throw new ArgumentException("User ID must be positive", nameof(userId));

        if (roleId <= 0)
            throw new ArgumentException("Role ID must be positive", nameof(roleId));

        if (expiresAt.HasValue && expiresAt.Value <= TimeNow.Now)
            throw new ArgumentException("Expiration date must be in the future", nameof(expiresAt));

        return new UserRole
        {
            UserId = userId,
            RoleId = roleId,
            AssignedAt = TimeNow.Now,
            ExpiresAt = expiresAt,
            IsActive = true
        };
    }

    /// <summary>
    /// Checks if the role assignment has expired.
    /// </summary>
    public bool HasExpired()
    {
        return ExpiresAt.HasValue && ExpiresAt.Value < TimeNow.Now;
    }

    /// <summary>
    /// Checks if the role assignment is currently effective.
    /// </summary>
    public bool IsEffective()
    {
        return IsActive && !HasExpired();
    }

    /// <summary>
    /// Activates the role assignment.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Deactivates the role assignment.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Extends the expiration date.
    /// </summary>
    public void ExtendExpiration(DateTime newExpiresAt)
    {
        if (newExpiresAt <= TimeNow.Now)
            throw new ArgumentException("New expiration date must be in the future", nameof(newExpiresAt));

        ExpiresAt = newExpiresAt;
    }

    /// <summary>
    /// Removes the expiration date (makes permanent).
    /// </summary>
    public void RemoveExpiration()
    {
        ExpiresAt = null;
    }

    /// <summary>
    /// Sets the user ID (used when adding to a user).
    /// </summary>
    internal void SetUser(long userId)
    {
        UserId = userId;
    }
}
