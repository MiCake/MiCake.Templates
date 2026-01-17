using System.ComponentModel.DataAnnotations;

namespace RBACWeb.Domain.Models.Authorization;

/// <summary>
/// Represents a role in the RBAC system.
/// Roles are collections of permissions that can be assigned to users.
/// </summary>
public class Role : AuditAggregateRoot
{
    /// <summary>
    /// Unique role code (e.g., "ADMIN", "USER", "REGIONAL_MANAGER").
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Code { get; private set; } = null!;

    /// <summary>
    /// Display name of the role.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Optional description of the role.
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; private set; }

    /// <summary>
    /// Whether this is a system built-in role that cannot be deleted.
    /// </summary>
    public bool IsSystem { get; private set; } = false;

    /// <summary>
    /// Whether the role is active.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    #region Navigation Properties

    /// <summary>
    /// Permissions assigned to this role.
    /// </summary>
    private readonly List<RolePermission> _rolePermissions = [];
    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    /// <summary>
    /// Data scopes assigned to this role.
    /// </summary>
    private readonly List<RoleDataScope> _roleDataScopes = [];
    public IReadOnlyCollection<RoleDataScope> RoleDataScopes => _roleDataScopes.AsReadOnly();

    #endregion

    protected Role() { }

    /// <summary>
    /// Creates a new role.
    /// </summary>
    public static Role Create(
        string code,
        string name,
        string? description = null,
        bool isSystem = false)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Role code cannot be empty", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name cannot be empty", nameof(name));

        return new Role
        {
            Code = code.ToUpperInvariant(),
            Name = name,
            Description = description,
            IsSystem = isSystem,
            IsActive = true
        };
    }

    /// <summary>
    /// Updates the role information.
    /// </summary>
    public void Update(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name cannot be empty", nameof(name));

        Name = name;
        Description = description;
    }

    /// <summary>
    /// Activates the role.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Deactivates the role. System roles cannot be deactivated.
    /// </summary>
    public void Deactivate()
    {
        if (IsSystem)
            throw new InvalidOperationException("System roles cannot be deactivated");

        IsActive = false;
    }

    #region Permission Management

    /// <summary>
    /// Adds a permission to the role.
    /// </summary>
    public void AddPermission(long permissionId, bool isGranted = true)
    {
        if (_rolePermissions.Any(rp => rp.PermissionId == permissionId))
            throw new InvalidOperationException($"Permission {permissionId} is already assigned to this role");

        var rolePermission = RolePermission.Create(Id, permissionId, isGranted);
        rolePermission.SetRole(Id);
        _rolePermissions.Add(rolePermission);
    }

    /// <summary>
    /// Removes a permission from the role.
    /// </summary>
    public void RemovePermission(long permissionId)
    {
        var rolePermission = _rolePermissions.FirstOrDefault(rp => rp.PermissionId == permissionId)
            ?? throw new InvalidOperationException($"Permission {permissionId} is not assigned to this role");

        _rolePermissions.Remove(rolePermission);
    }

    /// <summary>
    /// Checks if the role has a specific permission.
    /// </summary>
    public bool HasPermission(long permissionId)
    {
        return _rolePermissions.Any(rp => rp.PermissionId == permissionId && rp.IsGranted);
    }

    /// <summary>
    /// Gets all granted permission IDs.
    /// </summary>
    public IEnumerable<long> GetGrantedPermissionIds()
    {
        return _rolePermissions.Where(rp => rp.IsGranted).Select(rp => rp.PermissionId);
    }

    #endregion

    #region Data Scope Management

    /// <summary>
    /// Adds a data scope to the role.
    /// </summary>
    public void AddDataScope(long dataScopeId)
    {
        if (_roleDataScopes.Any(rds => rds.DataScopeId == dataScopeId))
            throw new InvalidOperationException($"DataScope {dataScopeId} is already assigned to this role");

        var roleDataScope = RoleDataScope.Create(Id, dataScopeId);
        roleDataScope.SetRole(Id);
        _roleDataScopes.Add(roleDataScope);
    }

    /// <summary>
    /// Removes a data scope from the role.
    /// </summary>
    public void RemoveDataScope(long dataScopeId)
    {
        var roleDataScope = _roleDataScopes.FirstOrDefault(rds => rds.DataScopeId == dataScopeId)
            ?? throw new InvalidOperationException($"DataScope {dataScopeId} is not assigned to this role");

        _roleDataScopes.Remove(roleDataScope);
    }

    /// <summary>
    /// Gets all data scope IDs assigned to this role.
    /// </summary>
    public IEnumerable<long> GetDataScopeIds()
    {
        return _roleDataScopes.Select(rds => rds.DataScopeId);
    }

    #endregion
}
