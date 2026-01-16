using Microsoft.AspNetCore.Mvc;
using MiCake.AspNetCore.Uow;
using StandardWeb.Application.Services.Configuration;
using StandardWeb.Contracts.Dtos.Configuration;
using StandardWeb.Domain.Enums.Configuration;

namespace StandardWeb.Web.Controllers;

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
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of settings with metadata</returns>
    [HttpGet("{settingGroup}")]
    [ProducesResponseType(typeof(List<AppSettingDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGroupSettings(
        string settingGroup,
        CancellationToken ct)
    {
        _logger.LogInformation("Getting settings for group: {Group}", settingGroup);

        if (!Enum.TryParse<SettingGroup>(settingGroup, ignoreCase: true, out var group))
            return BadRequest("INVALID_GROUP", $"Invalid setting group: {settingGroup}");

        var result = await _service.GetGroupSettingsAsync(group, ct);

        if (!result.IsSuccess)
            return BadRequest(result.ErrorCode, result.ErrorMessage);

        return Ok(result.Data);
    }

    /// <summary>
    /// Gets a specific setting by group and key.
    /// </summary>
    /// <param name="settingGroup">Setting group name</param>
    /// <param name="key">Setting key</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Setting with metadata</returns>
    [HttpGet("{settingGroup}/{key}")]
    [ProducesResponseType(typeof(AppSettingDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSetting(
        string settingGroup,
        string key,
        CancellationToken ct)
    {
        _logger.LogInformation("Getting setting: {Group}/{Key}", settingGroup, key);

        if (!Enum.TryParse<SettingGroup>(settingGroup, ignoreCase: true, out var group))
            return BadRequest("INVALID_GROUP", $"Invalid setting group: {settingGroup}");

        var result = await _service.GetSettingAsync(group, key, ct);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == ConfigurationErrorCodes.SettingNotFound)
                return NotFound();

            return BadRequest(result.ErrorCode ?? "ERROR", result.ErrorMessage);
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Creates a new configuration setting.
    /// </summary>
    /// <param name="dto">Setting creation data</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Created setting</returns>
    [HttpPost]
    [ProducesResponseType(typeof(AppSettingDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateSetting([FromBody] CreateAppSettingDto dto, CancellationToken ct)
    {
        _logger.LogInformation("Creating setting: {Group}/{Key}", dto.SettingGroup, dto.Key);

        var result = await _service.CreateSettingAsync(dto, ct);

        if (!result.IsSuccess)
            return BadRequest(result.ErrorCode ?? "ERROR", result.ErrorMessage);

        // Return 201 Created with location header
        return CreatedAtAction(
            nameof(GetSetting),
            new { settingGroup = dto.SettingGroup, key = dto.Key },
            result.Data);
    }

    /// <summary>
    /// Updates an existing setting value.
    /// </summary>
    /// <param name="settingGroup">Setting group name</param>
    /// <param name="key">Setting key</param>
    /// <param name="dto">Update data</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success or error</returns>
    [HttpPut("{settingGroup}/{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSetting(
        string settingGroup,
        string key,
        [FromBody] UpdateAppSettingDto dto,
        CancellationToken ct)
    {
        _logger.LogInformation("Updating setting: {Group}/{Key}", settingGroup, key);

        // Ensure route parameters match DTO
        if (!dto.SettingGroup.Equals(settingGroup, StringComparison.OrdinalIgnoreCase) ||
            !dto.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("PARAMETER_MISMATCH", "Route parameters must match request body");
        }

        var result = await _service.UpdateSettingAsync(dto, ct);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == ConfigurationErrorCodes.SettingNotFound)
                return NotFound();

            return BadRequest(result.ErrorCode ?? "ERROR", result.ErrorMessage);
        }

        return Ok(new { message = "Setting updated successfully" });
    }

    /// <summary>
    /// Deletes a configuration setting.
    /// </summary>
    /// <param name="settingGroup">Setting group name</param>
    /// <param name="key">Setting key</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success or error</returns>
    [HttpDelete("{settingGroup}/{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [UnitOfWork]
    public async Task<IActionResult> DeleteSetting(
        string settingGroup,
        string key,
        CancellationToken ct)
    {
        _logger.LogInformation("Deleting setting: {Group}/{Key}", settingGroup, key);

        if (!Enum.TryParse<SettingGroup>(settingGroup, ignoreCase: true, out var group))
            return BadRequest("INVALID_GROUP", $"Invalid setting group: {settingGroup}");

        var result = await _service.DeleteSettingAsync(group, key, ct);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == ConfigurationErrorCodes.SettingNotFound)
                return NotFound();

            if (result.ErrorCode == ConfigurationErrorCodes.CannotDeleteRequiredSetting)
                return BadRequest(result.ErrorCode, result.ErrorMessage);

            return BadRequest(result.ErrorCode ?? "ERROR", result.ErrorMessage);
        }

        return Ok(new { message = "Setting deleted successfully" });
    }

    /// <summary>
    /// Invalidates cache for a specific setting group.
    /// Useful for forcing cache refresh after bulk updates.
    /// </summary>
    /// <param name="settingGroup">Setting group name</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success message</returns>
    [HttpPost("{settingGroup}/invalidate-cache")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> InvalidateCache(
        string settingGroup,
        CancellationToken ct)
    {
        _logger.LogInformation("Invalidating cache for group: {Group}", settingGroup);

        if (!Enum.TryParse<SettingGroup>(settingGroup, ignoreCase: true, out var group))
            return BadRequest("INVALID_GROUP", $"Invalid setting group: {settingGroup}");

        await _service.InvalidateCacheAsync(group, ct);

        return Ok(new { message = "Cache invalidated successfully" });
    }

    private long? GetCurrentUserId()
    {
        var userIdClaim = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
