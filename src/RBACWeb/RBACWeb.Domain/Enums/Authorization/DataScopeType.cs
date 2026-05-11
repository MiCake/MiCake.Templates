namespace RBACWeb.Domain.Enums.Authorization;

/// <summary>
/// Defines the types of data access scopes for data-level permission control.
/// </summary>
public enum DataScopeType
{
    /// <summary>
    /// Access to all data in the system
    /// </summary>
    All = 1,

    /// <summary>
    /// Access only to data created by the user themselves
    /// </summary>
    Self = 2,

    /// <summary>
    /// Custom data scope defined by custom conditions
    /// </summary>
    Custom = 3
}
