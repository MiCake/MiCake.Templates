namespace RBACWeb.Contracts.Enums;

/// <summary>
/// Defines the types of actions that can be performed on a resource.
/// </summary>
public enum PermissionAction
{
    /// <summary>
    /// Read/View access to the resource
    /// </summary>
    Read = 1,

    /// <summary>
    /// Create new instances of the resource
    /// </summary>
    Create = 2,

    /// <summary>
    /// Update existing instances of the resource
    /// </summary>
    Update = 3,

    /// <summary>
    /// Delete instances of the resource
    /// </summary>
    Delete = 4,

    /// <summary>
    /// Execute/invoke the resource (for functions/operations)
    /// </summary>
    Execute = 5,

    /// <summary>
    /// Full management access to the resource
    /// </summary>
    Manage = 6,

    /// <summary>
    /// Export data from the resource
    /// </summary>
    Export = 7,

    /// <summary>
    /// Import data into the resource
    /// </summary>
    Import = 8
}
