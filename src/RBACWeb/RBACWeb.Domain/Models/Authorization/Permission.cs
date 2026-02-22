using System.ComponentModel.DataAnnotations;
using RBACWeb.Domain.Enums.Authorization;

namespace RBACWeb.Domain.Models.Authorization;

/// <summary>
/// Represents an atomic permission that can be granted to roles.
/// Permissions define what actions can be performed on a resource.
/// </summary>
public class Permission : AuditAggregateRoot
{
    /// <summary>
    /// Unique permission code (e.g., "user:read", "user:create").
    /// Format: {resource}:{action} or {module}:{resource}:{action}
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Code { get; private set; } = null!;

    /// <summary>
    /// Display name of the permission.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Optional description of the permission.
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; private set; }

    /// <summary>
    /// Optional resource this permission applies to.
    /// </summary>
    public long? ResourceId { get; private set; }

    /// <summary>
    /// The action type this permission grants.
    /// </summary>
    public PermissionAction Action { get; private set; }

    /// <summary>
    /// Whether the permission is active.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    #region Navigation Properties

    /// <summary>
    /// Optional resource this permission applies to.
    /// </summary>
    public Resource? Resource { get; private set; }

    /// <summary>
    /// Roles that have this permission.
    /// </summary>
    private readonly List<RolePermission> _rolePermissions = [];
    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    #endregion

    protected Permission() { }

    /// <summary>
    /// Creates a new permission.
    /// </summary>
    public static Permission Create(
        string code,
        string name,
        long? resourceId,
        PermissionAction action,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Permission code cannot be empty", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Permission name cannot be empty", nameof(name));

        if (resourceId.HasValue && resourceId.Value <= 0)
            throw new ArgumentException("Resource ID must be positive", nameof(resourceId));

        return new Permission
        {
            Code = code,
            Name = name,
            ResourceId = resourceId,
            Action = action,
            Description = description,
            IsActive = true
        };
    }

    /// <summary>
    /// Updates the permission information.
    /// </summary>
    public void Update(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Permission name cannot be empty", nameof(name));

        Name = name;
        Description = description;
    }

    /// <summary>
    /// Activates the permission.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Deactivates the permission.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }
}
