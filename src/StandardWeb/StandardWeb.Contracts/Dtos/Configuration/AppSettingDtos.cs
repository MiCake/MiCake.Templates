namespace StandardWeb.Contracts.Dtos.Configuration;

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
