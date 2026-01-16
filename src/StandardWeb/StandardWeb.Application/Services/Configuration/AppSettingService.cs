using AutoMapper;
using MiCake.Util.Convert;
using Microsoft.Extensions.Configuration;
using StandardWeb.Application.Cache;
using StandardWeb.Application.Constants;
using StandardWeb.Application.ErrorCodes;
using StandardWeb.Common.Security;
using StandardWeb.Contracts.Dtos.Configuration;
using StandardWeb.Domain.Enums.Configuration;
using StandardWeb.Domain.Models.Configuration;
using StandardWeb.Domain.Repositories;

namespace StandardWeb.Application.Services.Configuration;

/// <summary>
/// Unified configuration service for managing and reading dynamic settings.
/// Handles CRUD operations, caching, encryption, validation, and type-safe access.
/// Combines management and consumption into a single cohesive service.
/// </summary>
[InjectService(Lifetime = MiCakeServiceLifetime.Scoped)]
public class AppSettingService
{
    private readonly IAppSettingRepo _repository;
    private readonly ICacheService _cacheService;
    private readonly IDataProtectionService _dataProtectionService;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;
    private readonly ILogger<AppSettingService> _logger;

    // Cache duration from configuration (default: 10 minutes)
    private int CacheDurationMinutes => _configuration.GetValue("CacheSettings:AppSettingsCacheDurationMinutes", 10);

    public AppSettingService(
      IAppSettingRepo repository,
        ICacheService cacheService,
        IDataProtectionService dataProtectionService,
        IConfiguration configuration,
        IMapper mapper,
        ILogger<AppSettingService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _dataProtectionService = dataProtectionService ?? throw new ArgumentNullException(nameof(dataProtectionService));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new setting with validation and encryption.
    /// </summary>
    public async Task<OperationResult<AppSettingDto>> CreateSettingAsync(CreateAppSettingDto dto, CancellationToken ct = default)
    {
        try
        {
            if (!Enum.TryParse<SettingGroup>(dto.SettingGroup, ignoreCase: true, out var settingGroup))
                return OperationResult<AppSettingDto>.Failure(ConfigurationErrorCodes.InvalidValue, $"Invalid setting group: {dto.SettingGroup}");

            if (!Enum.TryParse<SettingDataType>(dto.DataType, ignoreCase: true, out var dataType))
                return OperationResult<AppSettingDto>.Failure(ConfigurationErrorCodes.InvalidDataType, $"Invalid data type: {dto.DataType}");

            if (await _repository.ExistsAsync(settingGroup, dto.Key, ct))
                return OperationResult<AppSettingDto>.Failure(ConfigurationErrorCodes.SettingAlreadyExists, $"Setting '{settingGroup}/{dto.Key}' already exists");

            var valueForStorage = PrepareValueForStorage(dto.Value, dto.IsEncrypted, DataEncrytionPurpose.AppSettingValue);
            if (valueForStorage == null)
                return OperationResult<AppSettingDto>.Failure(ConfigurationErrorCodes.EncryptionFailed, "Failed to encrypt value");

            var setting = AppSetting.Create(
                settingGroup,
                dto.Key,
                valueForStorage,
                dataType,
                dto.IsEncrypted,
                dto.Description,
                dto.ValidationPattern);

            await _repository.AddAsync(setting, ct);
            await _repository.SaveChangesAsync(ct);

            _logger.LogInformation("Created setting: {Group}/{Key}", settingGroup, dto.Key);

            return OperationResult<AppSettingDto>.Success(MapToDto(setting));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating setting {Group}/{Key}", dto.SettingGroup, dto.Key);
            return OperationResult<AppSettingDto>.Failure("INTERNAL_ERROR", "An error occurred while creating the setting");
        }
    }

    /// <summary>
    /// Updates an existing setting value with validation.
    /// </summary>
    public async Task<OperationResult> UpdateSettingAsync(UpdateAppSettingDto dto, CancellationToken ct = default)
    {
        try
        {
            if (!Enum.TryParse<SettingGroup>(dto.SettingGroup, ignoreCase: true, out var settingGroup))
                return OperationResult.Failure(ConfigurationErrorCodes.InvalidValue, $"Invalid setting group: {dto.SettingGroup}");

            var setting = await _repository.GetByKeyAsync(settingGroup, dto.Key, needTracking: true, ct);
            if (setting is null)
                return OperationResult.Failure(ConfigurationErrorCodes.SettingNotFound, $"Setting '{settingGroup}/{dto.Key}' not found");

            var valueForStorage = PrepareValueForStorage(dto.Value, setting.IsEncrypted, DataEncrytionPurpose.AppSettingValue);
            if (valueForStorage is null)
                return OperationResult.Failure(ConfigurationErrorCodes.EncryptionFailed, "Failed to encrypt value");

            setting.UpdateValue(valueForStorage);
            await _repository.SaveChangesAsync(ct);

            _logger.LogInformation("Updated setting: {Group}/{Key}", settingGroup, dto.Key);

            // Invalidate cache
            await InvalidateCacheAsync(settingGroup, dto.Key, ct);

            return OperationResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating setting {Group}/{Key}", dto.SettingGroup, dto.Key);
            return OperationResult.Failure("INTERNAL_ERROR", "An error occurred while updating the setting");
        }
    }

    /// <summary>
    /// Deletes a setting and invalidates cache.
    /// </summary>
    public async Task<OperationResult> DeleteSettingAsync(SettingGroup settingGroup, string key, CancellationToken ct = default)
    {
        try
        {
            var setting = await _repository.GetByKeyAsync(settingGroup, key, needTracking: true, ct);
            if (setting is null)
                return OperationResult.Failure(ConfigurationErrorCodes.SettingNotFound, $"Setting '{settingGroup}/{key}' not found");

            await _repository.DeleteByIdAsync(setting.Id, ct);
            await _repository.SaveChangesAsync(ct);

            _logger.LogInformation("Deleted setting: {Group}/{Key}", settingGroup, key);

            // Invalidate cache
            await InvalidateCacheAsync(settingGroup, key, ct);

            return OperationResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting setting {Group}/{Key}", settingGroup, key);
            return OperationResult.Failure("INTERNAL_ERROR", "An error occurred while deleting the setting");
        }
    }

    /// <summary>
    /// Gets a single setting value with automatic decryption and type conversion. only for simple types.
    /// Uses cache-aside pattern for performance.
    /// </summary>
    public async Task<T?> GetSettingValueAsync<T>(SettingGroup settingGroup, string key, CancellationToken ct = default) where T : notnull
    {
        try
        {
            // Try cache first
            var cacheKey = GetCacheKey(settingGroup, key);
            var cachedValue = await _cacheService.GetAsync<string>(cacheKey, ct);

            string? decryptedValue;

            if (cachedValue != null)
            {
                decryptedValue = cachedValue;
                _logger.LogDebug("Cache hit for setting: {Group}/{Key}", settingGroup, key);
            }
            else
            {
                var setting = await _repository.GetByKeyAsync(settingGroup, key, needTracking: false, ct);
                if (setting is null)
                {
                    _logger.LogWarning("Setting not found: {Group}/{Key}", settingGroup, key);
                    return default;
                }
                decryptedValue = GetDecryptedValue(setting);

                await _cacheService.SetAsync(cacheKey, decryptedValue, TimeSpan.FromMinutes(CacheDurationMinutes), ct);
                _logger.LogDebug("Cached setting: {Group}/{Key}", settingGroup, key);
            }

            return ConvertValue<T>(decryptedValue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting setting value {Group}/{Key}", settingGroup, key);
            return default;
        }
    }

    /// <summary>
    /// Gets all settings in a group as a dictionary (decrypted).
    /// Cached at group level.
    /// </summary>
    public async Task<Dictionary<string, object>> GetGroupValuesAsync(SettingGroup settingGroup, CancellationToken ct = default)
    {
        try
        {
            var settings = await _repository.GetByGroupAsync(settingGroup, needTracking: false, ct);

            var result = new Dictionary<string, object>();
            foreach (var setting in settings)
            {
                var decryptedValue = GetDecryptedValue(setting);
                var convertedValue = ConvertValueByDataType(decryptedValue, setting.DataType);
                result[setting.Key] = convertedValue;
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting setting group values: {Group}", settingGroup);
            return new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Gets all settings in a group as a strongly-typed object (RECOMMENDED).
    /// Automatically maps setting keys to object properties.
    /// </summary>
    public async Task<TSettings> GetGroupAsObjectAsync<TSettings>(SettingGroup settingGroup, CancellationToken ct = default)
        where TSettings : class, new()
    {
        var values = await GetGroupValuesAsync(settingGroup, ct);

        var result = new TSettings();
        var properties = typeof(TSettings).GetProperties();

        foreach (var prop in properties)
        {
            if (values.TryGetValue(prop.Name, out var value))
            {
                var convertedValue = Convert.ChangeType(value, prop.PropertyType);
                prop.SetValue(result, convertedValue);
            }
        }

        return result;
    }

    /// <summary>
    /// Gets setting metadata
    /// </summary>
    public async Task<OperationResult<AppSettingDto>> GetSettingWithoutCacheAsync(SettingGroup settingGroup, string key, CancellationToken ct = default)
    {
        try
        {
            var setting = await _repository.GetByKeyAsync(settingGroup, key, needTracking: false, ct);
            if (setting is null)
                return OperationResult<AppSettingDto>.Failure(ConfigurationErrorCodes.SettingNotFound, $"Setting '{settingGroup}/{key}' not found");

            return OperationResult<AppSettingDto>.Success(MapToDto(setting));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting setting {Group}/{Key}", settingGroup, key);
            return OperationResult<AppSettingDto>.Failure("INTERNAL_ERROR", "An error occurred");
        }
    }

    #region Cache Management

    /// <summary>
    /// Invalidates cache for a specific setting group.
    /// </summary>
    public async Task InvalidateCacheAsync(SettingGroup settingGroup, string key, CancellationToken ct = default)
    {
        try
        {
            var settingKey = GetCacheKey(settingGroup, key);
            await _cacheService.RemoveAsync(settingKey, ct);

            _logger.LogInformation("Invalidated cache for setting group: {Group}", settingGroup);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache for group {Group}", settingGroup);
        }
    }

    #endregion

    #region Private Helpers

    private string? PrepareValueForStorage(string value, bool shouldEncrypt, string purpose)
    {
        if (!shouldEncrypt)
            return value;

        try
        {
            return _dataProtectionService.Protect(value, purpose);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to encrypt value");
            return null;
        }
    }

    private string GetDecryptedValue(AppSetting setting)
    {
        if (!setting.IsEncrypted)
            return setting.Value;

        try
        {
            return _dataProtectionService.Unprotect(setting.Value, "AppSettings");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decrypt setting {Group}/{Key}", setting.SettingGroup, setting.Key);
            return setting.Value; // Return encrypted value as fallback
        }
    }

    private static string GetCacheKey(SettingGroup group, string key) => string.Format(CacheKeys.AppSettingByKey, group, key);

    private AppSettingDto MapToDto(AppSetting setting)
    {
        // Decrypt value for DTO
        var decryptedValue = GetDecryptedValue(setting);
        var result = _mapper.Map<AppSettingDto>(setting);

        return result with { Value = decryptedValue };
    }

    private T? ConvertValue<T>(string? value) where T : notnull
    {
        if (value == null)
            return default;

        try
        {
            return ValueConverter.Convert<string, T>(value);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to convert value '{Value}' to type {Type}", value, typeof(T).Name);
            return default;
        }
    }

    private static object ConvertValueByDataType(string value, SettingDataType dataType)
    {
        return dataType switch
        {
            SettingDataType.String => value,
            SettingDataType.Integer => int.TryParse(value, out var i) ? i : 0,
            SettingDataType.Boolean => bool.TryParse(value, out var b) && b,
            SettingDataType.Decimal => decimal.TryParse(value, out var d) ? d : 0m,
            _ => value
        };
    }

    #endregion
}
