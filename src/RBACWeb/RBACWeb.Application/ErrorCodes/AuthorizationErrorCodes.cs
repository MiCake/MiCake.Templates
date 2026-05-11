namespace RBACWeb.Application.ErrorCodes;

/// <summary>
/// Authorization (RBAC) related error codes.
/// </summary>
public class AuthorizationErrorCodes : BaseErrorCodes
{
    public const string RoleNotFound = "2000";
    public const string RoleAlreadyExists = "2001";
    public const string RoleIsSystem = "2002";
    public const string PermissionNotFound = "2010";
    public const string PermissionAlreadyExists = "2011";
    public const string ResourceNotFound = "2020";
    public const string ResourceAlreadyExists = "2021";
    public const string DataScopeNotFound = "2030";
    public const string DataScopeAlreadyExists = "2031";
    public const string UserRoleAlreadyAssigned = "2040";
    public const string UserRoleNotFound = "2041";
    public const string AccessDenied = "2050";
}
