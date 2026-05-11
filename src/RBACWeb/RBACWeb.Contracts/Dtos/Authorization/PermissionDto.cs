using RBACWeb.Contracts.Enums;

namespace RBACWeb.Contracts.Dtos.Authorization;

/// <summary>
/// Data transfer object for Permission.
/// </summary>
public class PermissionDto
{
    public long Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public long? ResourceId { get; set; }
    public string? ResourceName { get; set; }
    public PermissionAction Action { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Request DTO for creating a new permission.
/// </summary>
public class CreatePermissionDto
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public long? ResourceId { get; set; }
    public PermissionAction Action { get; set; }
}

/// <summary>
/// Request DTO for updating a permission.
/// </summary>
public class UpdatePermissionDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}
