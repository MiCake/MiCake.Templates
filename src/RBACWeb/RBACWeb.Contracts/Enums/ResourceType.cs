namespace RBACWeb.Contracts.Enums;

/// <summary>
/// Defines the type of resource being protected.
/// </summary>
public enum ResourceType
{
    /// <summary>
    /// A functional module in the system
    /// </summary>
    Module = 1,

    /// <summary>
    /// A menu item in the navigation
    /// </summary>
    Menu = 2,

    /// <summary>
    /// An API endpoint
    /// </summary>
    API = 3,

    /// <summary>
    /// A UI button or action
    /// </summary>
    Button = 4,

    /// <summary>
    /// A data entity or record type
    /// </summary>
    Data = 5
}
