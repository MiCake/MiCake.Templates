using System.ComponentModel.DataAnnotations;
using RBACWeb.Domain.Enums.Authorization;

namespace RBACWeb.Domain.Models.Authorization;

/// <summary>
/// Represents a protected system resource (module, API endpoint, menu, etc.).
/// Resources form a hierarchy and are associated with permissions.
/// </summary>
public class Resource : AuditAggregateRoot
{
    /// <summary>
    /// Unique resource code (e.g., "module:user-management", "api:users").
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Code { get; private set; } = null!;

    /// <summary>
    /// Display name of the resource.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Optional description of the resource.
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; private set; }

    /// <summary>
    /// Type of the resource (Module, Menu, API, Button, Data).
    /// </summary>
    public ResourceType Type { get; private set; }

    /// <summary>
    /// Parent resource ID for hierarchical organization.
    /// </summary>
    public long? ParentId { get; private set; }

    /// <summary>
    /// Resource path (URL for API, route for Menu).
    /// </summary>
    [MaxLength(500)]
    public string? Path { get; private set; }

    /// <summary>
    /// Display order for sorting.
    /// </summary>
    public int SortOrder { get; private set; } = 0;

    /// <summary>
    /// Whether the resource is active.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    #region Navigation Properties

    /// <summary>
    /// Parent resource in the hierarchy.
    /// </summary>
    public Resource? Parent { get; private set; }

    /// <summary>
    /// Child resources in the hierarchy.
    /// </summary>
    private readonly List<Resource> _children = [];
    public IReadOnlyCollection<Resource> Children => _children.AsReadOnly();

    /// <summary>
    /// Permissions associated with this resource.
    /// </summary>
    private readonly List<Permission> _permissions = [];
    public IReadOnlyCollection<Permission> Permissions => _permissions.AsReadOnly();

    #endregion

    protected Resource() { }

    /// <summary>
    /// Creates a new resource.
    /// </summary>
    public static Resource Create(
        string code,
        string name,
        ResourceType type,
        string? description = null,
        string? path = null,
        long? parentId = null,
        int sortOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Resource code cannot be empty", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Resource name cannot be empty", nameof(name));

        return new Resource
        {
            Code = code,
            Name = name,
            Type = type,
            Description = description,
            Path = path,
            ParentId = parentId,
            SortOrder = sortOrder,
            IsActive = true
        };
    }

    /// <summary>
    /// Updates the resource information.
    /// </summary>
    public void Update(string name, string? description, string? path, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Resource name cannot be empty", nameof(name));

        Name = name;
        Description = description;
        Path = path;
        SortOrder = sortOrder;
    }

    /// <summary>
    /// Activates the resource.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Deactivates the resource.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Sets the parent resource.
    /// </summary>
    public void SetParent(long? parentId)
    {
        if (parentId == Id)
            throw new InvalidOperationException("A resource cannot be its own parent");

        ParentId = parentId;
    }
}
