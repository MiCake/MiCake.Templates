namespace RBACWeb.Domain.Models.Authorization;

/// <summary>
/// Represents the association between a Role and a DataScope.
/// This is a child entity of the Role aggregate.
/// </summary>
public class RoleDataScope : AuditEntity
{
    /// <summary>
    /// The role that has this data scope.
    /// </summary>
    public long RoleId { get; private set; }

    /// <summary>
    /// The data scope assigned to the role.
    /// </summary>
    public long DataScopeId { get; private set; }

    #region Navigation Properties

    /// <summary>
    /// The role that has this data scope.
    /// </summary>
    public Role Role { get; private set; } = null!;

    /// <summary>
    /// The data scope assigned to the role.
    /// </summary>
    public DataScope DataScope { get; private set; } = null!;

    #endregion

    protected RoleDataScope() { }

    /// <summary>
    /// Creates a new role-datascope association.
    /// </summary>
    internal static RoleDataScope Create(long roleId, long dataScopeId)
    {
        if (roleId <= 0)
            throw new ArgumentException("Role ID must be positive", nameof(roleId));

        if (dataScopeId <= 0)
            throw new ArgumentException("DataScope ID must be positive", nameof(dataScopeId));

        return new RoleDataScope
        {
            RoleId = roleId,
            DataScopeId = dataScopeId
        };
    }

    /// <summary>
    /// Sets the role ID (used when adding to a role).
    /// </summary>
    internal void SetRole(long roleId)
    {
        RoleId = roleId;
    }
}
