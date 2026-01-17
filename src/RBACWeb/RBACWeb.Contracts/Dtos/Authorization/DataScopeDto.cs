using RBACWeb.Contracts.Enums;

namespace RBACWeb.Contracts.Dtos.Authorization;

/// <summary>
/// Data transfer object for DataScope.
/// </summary>
public class DataScopeDto
{
    public long Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DataScopeType Type { get; set; }
    public string? Condition { get; set; }
    public int Priority { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Request DTO for creating a new data scope.
/// </summary>
public class CreateDataScopeDto
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DataScopeType Type { get; set; }
    public string? Condition { get; set; }
    public int Priority { get; set; }
}

/// <summary>
/// Request DTO for updating a data scope.
/// </summary>
public class UpdateDataScopeDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Condition { get; set; }
    public int Priority { get; set; }
}
