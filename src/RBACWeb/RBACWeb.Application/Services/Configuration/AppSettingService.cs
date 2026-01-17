using AutoMapper;
using MiCake.Util.Convert;
using Microsoft.Extensions.Configuration;
using RBACWeb.Application.Cache;
using RBACWeb.Application.Constants;
using RBACWeb.Application.ErrorCodes;
using RBACWeb.Common.Security;
using RBACWeb.Contracts.Dtos.Configuration;
using RBACWeb.Domain.Enums.Configuration;
using RBACWeb.Domain.Models.Configuration;
using RBACWeb.Domain.Repositories;

namespace RBACWeb.Application.Services.Configuration;

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
        _logger.LogInformation("Created setting: {Group}/{Key}", settingGroup, dto.Key);

        return OperationResult<AppSettingDto>.Success(MapToDto(setting));
    }

    /// <summary>
    /// Updates an existing setting value with validation.
    /// </summary>
    public async Task<OperationResult> UpdateSettingAsync(UpdateAppSettingDto dto, CancellationToken ct = default)
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

        _logger.LogInformation("Updated setting: {Group}/{Key}", settingGroup, dto.Key);

        // Invalidate cache
        await InvalidateCacheAsync(settingGroup, dto.Key, ct);

        return OperationResult.Success();
    }

    /// <summary>
    /// Deletes a setting and invalidates cache.
    /// </summary>
    public async Task<OperationResult> DeleteSettingAsync(SettingGroup settingGroup, string key, CancellationToken ct = default)
    {
        var setting = await _repository.GetByKeyAsync(settingGroup, key, needTracking: true, ct);
        if (setting is null)
            return OperationResult.Failure(ConfigurationErrorCodes.SettingNotFound, $"Setting '{settingGroup}/{key}' not found");

        await _repository.DeleteByIdAsync(setting.Id, ct);

        _logger.LogInformation("Deleted setting: {Group}/{Key}", settingGroup, key);

        // Invalidate cache
        await InvalidateCacheAsync(settingGroup, key, ct);

        return OperationResult.Success();
    }

    /// <summary>
    /// Batch upserts (create or update) multiple settings in a single group.
    /// </summary>
    public async Task<OperationResult<BatchUpsertResultDto>> BatchUpsertSettingsAsync(BatchUpsertAppSettingsDto dto, CancellationToken ct = default)
    {
        if (!Enum.TryParse<SettingGroup>(dto.SettingGroup, ignoreCase: true, out var settingGroup))
            return OperationResult<BatchUpsertResultDto>.Failure(ConfigurationErrorCodes.InvalidValue, $"Invalid setting group: {dto.SettingGroup}");

        if (dto.Settings == null || dto.Settings.Count == 0)
            return OperationResult<BatchUpsertResultDto>.Failure(ConfigurationErrorCodes.InvalidInput, "Settings list cannot be empty");

        var result = new BatchUpsertResultDto
        {
            SettingGroup = dto.SettingGroup,
            TotalProcessed = dto.Settings.Count
        };

        // Get all existing settings in the group
        var existingSettings = await _repository.GetByGroupAsync(settingGroup, needTracking: true, ct);
        var existingDict = existingSettings.ToDictionary(s => s.Key, StringComparer.OrdinalIgnoreCase);

        foreach (var item in dto.Settings)
        {
            try
            {
                if (existingDict.TryGetValue(item.Key, out var existingSetting))
                {
                    // Update existing setting
                    var valueForStorage = PrepareValueForStorage(item.Value, existingSetting.IsEncrypted, DataEncrytionPurpose.AppSettingValue);
                    if (valueForStorage is null)
                    {
                        result.Failed++;
                        result.Errors.Add($"{item.Key}: Failed to encrypt value");
                        continue;
                    }

                    existingSetting.UpdateValue(valueForStorage);
                    result.Updated++;
                    _logger.LogInformation("Updated setting in batch: {Group}/{Key}", settingGroup, item.Key);

                    // Invalidate cache
                    await InvalidateCacheAsync(settingGroup, item.Key, ct);
                }
                else
                {
                    // Create new setting
                    if (!Enum.TryParse<SettingDataType>(item.DataType, ignoreCase: true, out var dataType))
                    {
                        result.Failed++;
                        result.Errors.Add($"{item.Key}: Invalid data type '{item.DataType}'");
                        continue;
                    }

                    var valueForStorage = PrepareValueForStorage(item.Value, item.IsEncrypted, DataEncrytionPurpose.AppSettingValue);
                    if (valueForStorage is null)
                    {
                        result.Failed++;
                        result.Errors.Add($"{item.Key}: Failed to encrypt value");
                        continue;
                    }

                    var newSetting = AppSetting.Create(
                        settingGroup,
                        item.Key,
                        valueForStorage,
                        dataType,
                        item.IsEncrypted,
                        item.Description,
                        item.ValidationPattern);

                    await _repository.AddAsync(newSetting, ct);
                    result.Created++;
                    _logger.LogInformation("Created setting in batch: {Group}/{Key}", settingGroup, item.Key);
                }
            }
            catch (Exception ex)
            {
                result.Failed++;
                result.Errors.Add($"{item.Key}: {ex.Message}");
                _logger.LogError(ex, "Error processing setting in batch: {Group}/{Key}", settingGroup, item.Key);
            }
        }

        _logger.LogInformation("Batch upsert completed for group {Group}: Created={Created}, Updated={Updated}, Failed={Failed}",
            settingGroup, result.Created, result.Updated, result.Failed);

        return OperationResult<BatchUpsertResultDto>.Success(result);
    }

    /// <summary>
    /// Gets a single setting value with automatic decryption and type conversion. only for simple types.
    /// </summary>
    public async Task<T?> GetSettingValueAsync<T>(SettingGroup settingGroup, string key, CancellationToken ct = default) where T : notnull
    {
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

    /// <summary>
    /// Gets all settings in a group as DTOs.
    /// </summary>
    public async Task<List<AppSettingDto>> GetGroupSettingsAsync(SettingGroup settingGroup, CancellationToken ct = default)
    {
        var settings = await _repository.GetByGroupAsync(settingGroup, needTracking: false, ct);

        var result = settings?.Select(MapToDto).ToList();
        return result ?? [];
    }

    /// <summary>
    /// Gets all settings in a group as a dictionary with key-value pairs.
    /// Values are automatically decrypted and converted to their appropriate data types.
    /// </summary>
    public async Task<Dictionary<string, object>> GetGroupValuesAsync(SettingGroup settingGroup, CancellationToken ct = default)
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

    /// <summary>
    /// Gets all settings in a group as a strongly-typed object.
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
    /// Gets setting metadata without using cache.
    /// </summary>
    public async Task<OperationResult<AppSettingDto>> GetSettingWithoutCacheAsync(SettingGroup settingGroup, string key, CancellationToken ct = default)
    {
        var setting = await _repository.GetByKeyAsync(settingGroup, key, needTracking: false, ct);
        if (setting is null)
            return OperationResult<AppSettingDto>.Failure(ConfigurationErrorCodes.SettingNotFound, $"Setting '{settingGroup}/{key}' not found");

        return OperationResult<AppSettingDto>.Success(MapToDto(setting));
    }

    #region Cache Management

    /// <summary>
    /// Invalidates cache for a specific setting.
    /// Called automatically after create, update, or delete operations.
    /// </summary>
    public async Task InvalidateCacheAsync(SettingGroup settingGroup, string key, CancellationToken ct = default)
    {
        try
        {
            var settingKey = GetCacheKey(settingGroup, key);
            await _cacheService.RemoveAsync(settingKey, ct);

            _logger.LogInformation("Invalidated cache for setting: {Group}/{Key}", settingGroup, key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache for setting: {Group}/{Key}", settingGroup, key);
        }
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// Prepares a value for storage by encrypting it if required.
    /// </summary>
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

    /// <summary>
    /// Gets the decrypted value of a setting, or returns the plain value if not encrypted.
    /// </summary>
    private string GetDecryptedValue(AppSetting setting)
    {
        if (!setting.IsEncrypted)
            return setting.Value;

        try
        {
            return _dataProtectionService.Unprotect(setting.Value, DataEncrytionPurpose.AppSettingValue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decrypt setting {Group}/{Key}", setting.SettingGroup, setting.Key);
            throw;
        }
    }

    /// <summary>
    /// Generates a cache key for a specific setting.
    /// </summary>
    private static string GetCacheKey(SettingGroup group, string key) => string.Format(CacheKeys.AppSettingByKey, group, key);

    /// <summary>
    /// Maps an AppSetting entity to a DTO with decrypted value.
    /// </summary>
    private AppSettingDto MapToDto(AppSetting setting)
    {
        var decryptedValue = GetDecryptedValue(setting);
        var result = _mapper.Map<AppSettingDto>(setting);

        return result with { Value = decryptedValue };
    }

    /// <summary>
    /// Converts a string value to the specified type.
    /// </summary>
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

    /// <summary>
    /// Converts a string value based on its data type enumeration.
    /// </summary>
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
