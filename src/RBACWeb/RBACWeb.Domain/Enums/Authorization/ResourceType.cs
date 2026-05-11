namespace RBACWeb.Domain.Enums.Authorization;

/// <summary>
/// Defines the types of protected resources in the system.
/// </summary>
public enum ResourceType
{
    /// <summary>
    /// A system module or feature group.
    /// </summary>
    Module = 1,

    /// <summary>
    /// A navigation menu item.
    /// </summary>
    Menu = 2,

    /// <summary>
    /// An API endpoint.
    /// </summary>
    API = 3,

    /// <summary>
    /// A UI button or action.
    /// </summary>
    Button = 4,

    /// <summary>
    /// A data entity.
    /// </summary>
    Data = 5
}
