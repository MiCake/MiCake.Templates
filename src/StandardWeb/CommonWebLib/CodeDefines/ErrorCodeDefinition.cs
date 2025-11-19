namespace CommonWebLib.CodeDefines;

/// <summary>
/// Centralized error and success code definitions for API responses.
/// Format: ModuleCode.Code (e.g., "01.1000" for auth update failure, "01.0000" for success)
/// </summary>
public class ErrorCodeDefinition
{
    // Success codes (0000-0999)
    /// <summary>
    /// Operation completed successfully
    /// </summary>
    public const string Success = "0000";

    /// <summary>
    /// Resource retrieved successfully
    /// </summary>
    public const string GetSuccess = "0001";

    /// <summary>
    /// Resource created successfully
    /// </summary>
    public const string CreateSuccess = "0002";

    /// <summary>
    /// Resource updated successfully
    /// </summary>
    public const string UpdateSuccess = "0003";

    /// <summary>
    /// Resource deleted successfully
    /// </summary>
    public const string DeleteSuccess = "0004";

    // CRUD operation errors (1000-1999)
    /// <summary>
    /// Failed to update resource in database
    /// </summary>
    public const string UpdateInfoFailed = "1000";

    /// <summary>
    /// Failed to delete resource from database
    /// </summary>
    public const string DeleteInfoFailed = "1001";

    /// <summary>
    /// Failed to create new resource in database
    /// </summary>
    public const string CreateInfoFailed = "1002";

    /// <summary>
    /// Failed to retrieve resource from database
    /// </summary>
    public const string GetInfoFailed = "1003";

    /// <summary>
    /// Input validation failed or invalid request data
    /// </summary>
    public const string InvalidInput = "1004";

    /// <summary>
    /// Requested resource does not exist
    /// </summary>
    public const string NotFound = "1005";

    /// <summary>
    /// User lacks permission for the requested operation
    /// </summary>
    public const string OperationNotAllowed = "1006";

    /// <summary>
    /// Authentication required or authentication failed
    /// </summary>
    public const string Unauthorized = "1007";

    // System-level errors (9900-9999)
    /// <summary>
    /// Generic operation failure without specific category
    /// </summary>
    public const string OperationFailed = "9990";

    /// <summary>
    /// Request validation error (FluentValidation)
    /// </summary>
    public const string ValidationError = "9998";

    /// <summary>
    /// Unhandled application-level error
    /// </summary>
    public const string ApplicationError = "9999";
}
