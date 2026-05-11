namespace RBACWeb.Domain.Enums.Authorization;

/// <summary>
/// Defines the types of actions that can be performed on a resource.
/// </summary>
public enum PermissionAction
{
    /// <summary>
    /// View or list resources.
    /// </summary>
    Read = 1,

    /// <summary>
    /// Create new resources.
    /// </summary>
    Create = 2,

    /// <summary>
    /// Modify existing resources.
    /// </summary>
    Update = 3,

    /// <summary>
    /// Remove resources.
    /// </summary>
    Delete = 4,

    /// <summary>
    /// Execute an operation.
    /// </summary>
    Execute = 5,

    /// <summary>
    /// Full management access.
    /// </summary>
    Manage = 6,

    /// <summary>
    /// Export data.
    /// </summary>
    Export = 7,

    /// <summary>
    /// Import data.
    /// </summary>
    Import = 8
}
