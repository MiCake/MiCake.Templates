namespace RBACWeb.Contracts.Dtos.Configuration;

/// <summary>
/// DTO for AppSetting entity with all metadata.
/// Used for admin UI and detailed queries.
/// </summary>
public record AppSettingDto(
    long Id,
    string SettingGroup,        // Enum as string for API compatibility
    string Key,
    string Value,               // Decrypted when read from API
    string DataType,            // Enum as string
    string? Description,
    bool IsEncrypted,
    string? ValidationPattern,
    long? CreatedBy,
    DateTime CreatedAt,
    long? ModifiedBy,
    DateTime? UpdatedAt);

/// <summary>
/// DTO for creating a new AppSetting.
/// </summary>
public record CreateAppSettingDto(
    string SettingGroup,        // Will be parsed to SettingGroup enum
    string Key,
    string Value,               // Plain text - will be encrypted by service if needed
    string DataType,            // Will be parsed to SettingDataType enum
    string? Description,
    bool IsEncrypted = false,
    string? ValidationPattern = null);

/// <summary>
/// DTO for updating an existing AppSetting value.
/// </summary>
public record UpdateAppSettingDto(
    string SettingGroup,        // Will be parsed to enum
    string Key,
    string Value);              // Plain text

/// <summary>
/// DTO for updating setting metadata (description, validation pattern).
/// </summary>
public record UpdateAppSettingMetadataDto(
    string? Description,
    string? ValidationPattern);

/// <summary>
/// DTO for batch upsert (create or update) settings in a single group.
/// </summary>
public record BatchUpsertAppSettingsDto(
    string SettingGroup,        // All settings must belong to this group
    List<BatchSettingItemDto> Settings);

/// <summary>
/// Individual setting item for batch operations.
/// </summary>
public record BatchSettingItemDto(
    string Key,
    string Value,
    string DataType,            // Required for create, ignored for update
    string? Description = null,
    bool IsEncrypted = false,
    string? ValidationPattern = null);

/// <summary>
/// Result of batch upsert operation.
/// </summary>
public record BatchUpsertResultDto
{
    public string SettingGroup { get; set; } = string.Empty;
    public int TotalProcessed { get; set; }
    public int Created { get; set; }
    public int Updated { get; set; }
    public int Failed { get; set; }
    public List<string> Errors { get; set; } = new();
}
