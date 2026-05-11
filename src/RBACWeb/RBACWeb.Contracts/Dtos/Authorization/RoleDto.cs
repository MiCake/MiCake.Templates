namespace RBACWeb.Contracts.Dtos.Authorization;

/// <summary>
/// Data transfer object for Role.
/// </summary>
public class RoleDto
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

/// <summary>
/// Data transfer object for Role with permissions.
/// </summary>
public class RoleDetailDto : RoleDto
{
    public List<PermissionDto> Permissions { get; set; } = [];
    public List<DataScopeDto> DataScopes { get; set; } = [];
}

/// <summary>
/// Request DTO for creating a new role.
/// </summary>
public class CreateRoleDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}

/// <summary>
/// Request DTO for updating a role.
/// </summary>
public class UpdateRoleDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}

/// <summary>
/// Request DTO for assigning permissions to a role.
/// </summary>
public class AssignPermissionsDto
{
    public List<long> PermissionIds { get; set; } = [];
}

/// <summary>
/// Request DTO for assigning data scopes to a role.
/// </summary>
public class AssignDataScopesDto
{
    public List<long> DataScopeIds { get; set; } = [];
}
