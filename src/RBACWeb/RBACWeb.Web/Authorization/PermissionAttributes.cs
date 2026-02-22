using Microsoft.AspNetCore.Authorization;

namespace RBACWeb.Web.Authorization;

/// <summary>
/// Specifies that the decorated action or controller requires the specified permission.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequirePermissionAttribute : AuthorizeAttribute
{
    /// <summary>
    /// The permission code required to access the resource.
    /// </summary>
    public string PermissionCode { get; }

    public RequirePermissionAttribute(string permissionCode) : base(policy: $"Permission:{permissionCode}")
    {
        PermissionCode = permissionCode;
    }
}

