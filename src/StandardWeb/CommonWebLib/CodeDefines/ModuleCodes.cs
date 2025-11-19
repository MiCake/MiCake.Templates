namespace CommonWebLib.CodeDefines;

/// <summary>
/// Module code prefixes for error code categorization.
/// Each module has a unique two-digit identifier that prefixes error codes.
/// </summary>
public class ModuleCodes
{
    /// <summary>
    /// Authentication and authorization module (login, token refresh)
    /// </summary>
    public const string AuthModule = "01";

    /// <summary>
    /// User management module (registration, profile updates)
    /// </summary>
    public const string UserManagementModule = "02";
}
