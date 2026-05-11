namespace RBACWeb.Domain.Models.Authorization;

/// <summary>
/// Represents the association between a Role and a Permission.
/// This is a child entity of the Role aggregate.
/// </summary>
public class RolePermission : AuditEntity
{
    /// <summary>
    /// The role that has this permission.
    /// </summary>
    public long RoleId { get; private set; }

    /// <summary>
    /// The permission granted to the role.
    /// </summary>
    public long PermissionId { get; private set; }

    /// <summary>
    /// Whether the permission is granted (true) or explicitly denied (false).
    /// </summary>
    public bool IsGranted { get; private set; } = true;

    #region Navigation Properties

    /// <summary>
    /// The role that has this permission.
    /// </summary>
    public Role Role { get; private set; } = null!;

    /// <summary>
    /// The permission granted to the role.
    /// </summary>
    public Permission Permission { get; private set; } = null!;

    #endregion

    protected RolePermission() { }

    /// <summary>
    /// Creates a new role-permission association.
    /// </summary>
    internal static RolePermission Create(long roleId, long permissionId, bool isGranted = true)
    {
        if (roleId <= 0)
            throw new ArgumentException("Role ID must be positive", nameof(roleId));

        if (permissionId <= 0)
            throw new ArgumentException("Permission ID must be positive", nameof(permissionId));

        return new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId,
            IsGranted = isGranted
        };
    }

    /// <summary>
    /// Grants the permission.
    /// </summary>
    public void Grant()
    {
        IsGranted = true;
    }

    /// <summary>
    /// Denies the permission.
    /// </summary>
    public void Deny()
    {
        IsGranted = false;
    }

    /// <summary>
    /// Sets the role ID (used when adding to a role).
    /// </summary>
    internal void SetRole(long roleId)
    {
        RoleId = roleId;
    }
}
