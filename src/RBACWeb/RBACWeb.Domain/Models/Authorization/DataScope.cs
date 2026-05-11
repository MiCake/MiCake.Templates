using System.ComponentModel.DataAnnotations;
using RBACWeb.Domain.Enums.Authorization;

namespace RBACWeb.Domain.Models.Authorization;

/// <summary>
/// Represents a data access scope that defines boundaries for data-level permission control.
/// Data scopes determine what data a user can access based on their roles.
/// </summary>
public class DataScope : AuditAggregateRoot
{
    /// <summary>
    /// Unique scope code (e.g., "scope:all", "scope:self", "region:east").
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Code { get; private set; } = null!;

    /// <summary>
    /// Display name of the data scope.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Optional description of the data scope.
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; private set; }

    /// <summary>
    /// Type of data scope (All, Department, Region, Self, Custom).
    /// </summary>
    public DataScopeType Type { get; private set; }

    /// <summary>
    /// Custom filter condition for Custom type scopes.
    /// Can be a JSON expression or filter definition.
    /// </summary>
    public string? Condition { get; private set; }

    /// <summary>
    /// Priority when multiple scopes apply. Higher priority takes precedence.
    /// </summary>
    public int Priority { get; private set; } = 0;

    /// <summary>
    /// Whether the data scope is active.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    #region Navigation Properties

    /// <summary>
    /// Roles that have this data scope.
    /// </summary>
    private readonly List<RoleDataScope> _roleDataScopes = [];
    public IReadOnlyCollection<RoleDataScope> RoleDataScopes => _roleDataScopes.AsReadOnly();

    #endregion

    protected DataScope() { }

    /// <summary>
    /// Creates a new data scope.
    /// </summary>
    public static DataScope Create(
        string code,
        string name,
        DataScopeType type,
        string? description = null,
        string? condition = null,
        int priority = 0)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Data scope code cannot be empty", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Data scope name cannot be empty", nameof(name));

        // Custom type requires a condition
        if (type == DataScopeType.Custom && string.IsNullOrWhiteSpace(condition))
            throw new ArgumentException("Custom data scope requires a condition", nameof(condition));

        return new DataScope
        {
            Code = code,
            Name = name,
            Type = type,
            Description = description,
            Condition = condition,
            Priority = priority,
            IsActive = true
        };
    }

    /// <summary>
    /// Updates the data scope information.
    /// </summary>
    public void Update(string name, string? description, string? condition, int priority)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Data scope name cannot be empty", nameof(name));

        if (Type == DataScopeType.Custom && string.IsNullOrWhiteSpace(condition))
            throw new ArgumentException("Custom data scope requires a condition", nameof(condition));

        Name = name;
        Description = description;
        Condition = condition;
        Priority = priority;
    }

    /// <summary>
    /// Activates the data scope.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Deactivates the data scope.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }
}
