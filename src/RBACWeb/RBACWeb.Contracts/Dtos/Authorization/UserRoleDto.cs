namespace RBACWeb.Contracts.Dtos.Authorization;

/// <summary>
/// Data transfer object for user role assignment.
/// </summary>
public class UserRoleDto
{
    public long RoleId { get; set; }
    public string RoleCode { get; set; } = null!;
    public string RoleName { get; set; } = null!;
    public DateTime AssignedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Request DTO for assigning a role to a user.
/// </summary>
public class AssignUserRoleDto
{
    public long RoleId { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// DTO for user permissions summary.
/// </summary>
public class UserPermissionsDto
{
    public long UserId { get; set; }
    public List<string> RoleCodes { get; set; } = [];
    public List<string> PermissionCodes { get; set; } = [];
}
