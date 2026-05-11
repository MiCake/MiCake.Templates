using RBACWeb.Contracts.Enums;

namespace RBACWeb.Contracts.Dtos.Authorization;

/// <summary>
/// Data transfer object for Resource.
/// </summary>
public class ResourceDto
{
    public long Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public ResourceType Type { get; set; }
    public long? ParentId { get; set; }
    public string? Path { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Data transfer object for Resource with children.
/// </summary>
public class ResourceTreeDto : ResourceDto
{
    public List<ResourceTreeDto> Children { get; set; } = [];
}

/// <summary>
/// Request DTO for creating a new resource.
/// </summary>
public class CreateResourceDto
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public ResourceType Type { get; set; }
    public long? ParentId { get; set; }
    public string? Path { get; set; }
    public int SortOrder { get; set; }
}

/// <summary>
/// Request DTO for updating a resource.
/// </summary>
public class UpdateResourceDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Path { get; set; }
    public int SortOrder { get; set; }
}
