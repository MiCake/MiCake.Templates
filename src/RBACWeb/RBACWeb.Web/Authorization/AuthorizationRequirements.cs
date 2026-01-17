using Microsoft.AspNetCore.Authorization;

namespace RBACWeb.Web.Authorization;

/// <summary>
/// Requirement for permission-based authorization.
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    public string PermissionCode { get; }

    public PermissionRequirement(string permissionCode)
    {
        PermissionCode = permissionCode;
    }
}

/// <summary>
/// Requirement for role-based authorization.
/// </summary>
public class RoleRequirement : IAuthorizationRequirement
{
    public string RoleCode { get; }

    public RoleRequirement(string roleCode)
    {
        RoleCode = roleCode;
    }
}
