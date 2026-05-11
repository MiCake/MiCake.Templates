using Microsoft.AspNetCore.Mvc;
using RBACWeb.Application.Services.Configuration;
using RBACWeb.Contracts.Dtos.Configuration;
using RBACWeb.Domain.Enums.Configuration;

namespace RBACWeb.Web.Controllers;

/// <summary>
/// API controller for managing dynamic configuration settings.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class AppSettingsController : BaseApiController
{
    private readonly AppSettingService _service;
    private readonly ILogger<AppSettingsController> _logger;

    public AppSettingsController(
        AppSettingService service,
        InfrastructureTools infrastructureTools,
        ILogger<AppSettingsController> logger) : base(infrastructureTools)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        ModuleCode = ModuleCodes.ConfigurationModule;
    }

    /// <summary>
    /// Gets all settings in a specific group.
    /// </summary>
    /// <param name="settingGroup">Setting group name (Email, Sms, Payment, etc.)</param>
    /// <returns>List of settings with metadata</returns>
    [HttpGet("{settingGroup}")]
    [ProducesResponseType(typeof(List<AppSettingDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGroupSettings(string settingGroup)
    {
        _logger.LogInformation("Getting settings for group: {Group}", settingGroup);

        if (!Enum.TryParse<SettingGroup>(settingGroup, ignoreCase: true, out var group))
            return BadRequest(ConfigurationErrorCodes.GroupNotFound, $"Invalid setting group: {settingGroup}");

        var result = await _service.GetGroupSettingsAsync(group, HttpCancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets a specific setting by group and key.
    /// </summary>
    /// <param name="settingGroup">Setting group name</param>
    /// <param name="key">Setting key</param>
    /// <returns>Setting with metadata</returns>
    [HttpGet("{settingGroup}/{key}")]
    [ProducesResponseType(typeof(AppSettingDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSetting(string settingGroup, string key)
    {
        _logger.LogInformation("Getting setting: {Group}/{Key}", settingGroup, key);

        if (!Enum.TryParse<SettingGroup>(settingGroup, ignoreCase: true, out var group))
            return BadRequest(ConfigurationErrorCodes.GroupNotFound, $"Invalid setting group: {settingGroup}");

        var result = await _service.GetSettingWithoutCacheAsync(group, key, HttpCancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorCode!, result.ErrorMessage);
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Creates a new configuration setting.
    /// </summary>
    /// <param name="dto">Setting creation data</param>
    /// <returns>Created setting</returns>
    [HttpPost]
    [ProducesResponseType(typeof(AppSettingDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateSetting([FromBody] CreateAppSettingDto dto)
    {
        _logger.LogInformation("Creating setting: {Group}/{Key}", dto.SettingGroup, dto.Key);

        var result = await _service.CreateSettingAsync(dto, HttpCancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result.ErrorCode!, result.ErrorMessage);

        return Ok(result.Data);
    }

    /// <summary>
    /// Batch upserts (creates or updates) multiple settings in a single group.
    /// </summary>
    [HttpPost("batch")]
    [ProducesResponseType(typeof(BatchUpsertResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> BatchUpsertSettings([FromBody] BatchUpsertAppSettingsDto dto)
    {
        _logger.LogInformation("Batch upserting {Count} settings for group: {Group}", dto.Settings?.Count ?? 0, dto.SettingGroup);

        var result = await _service.BatchUpsertSettingsAsync(dto, HttpCancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorCode!, result.ErrorMessage);

        return Ok(result.Data);
    }

    /// <summary>
    /// Updates an existing setting value.
    /// </summary>
    /// <param name="settingGroup">Setting group name</param>
    /// <param name="key">Setting key</param>
    /// <param name="dto">Update data</param>
    /// <returns>Success or error</returns>
    [HttpPut("{settingGroup}/{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateSetting(string settingGroup, string key, [FromBody] UpdateAppSettingDto dto)
    {
        _logger.LogInformation("Updating setting: {Group}/{Key}", settingGroup, key);

        // Ensure route parameters match DTO
        if (!dto.SettingGroup.Equals(settingGroup, StringComparison.OrdinalIgnoreCase) ||
            !dto.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(ConfigurationErrorCodes.InvalidInput, "Route parameters must match request body");
        }

        var result = await _service.UpdateSettingAsync(dto, HttpCancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorCode!, result.ErrorMessage);
        }

        return Ok(true);
    }

    /// <summary>
    /// Deletes a configuration setting.
    /// </summary>
    /// <param name="settingGroup">Setting group name</param>
    /// <param name="key">Setting key</param>
    /// <returns>Success or error</returns>
    [HttpDelete("{settingGroup}/{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteSetting(string settingGroup, string key)
    {
        _logger.LogInformation("Deleting setting: {Group}/{Key}", settingGroup, key);

        if (!Enum.TryParse<SettingGroup>(settingGroup, ignoreCase: true, out var group))
            return BadRequest(ConfigurationErrorCodes.InvalidInput, $"Invalid setting group: {settingGroup}");

        var result = await _service.DeleteSettingAsync(group, key, HttpCancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorCode!, result.ErrorMessage);
        }

        return Ok(new { message = "Setting deleted successfully" });
    }
}
