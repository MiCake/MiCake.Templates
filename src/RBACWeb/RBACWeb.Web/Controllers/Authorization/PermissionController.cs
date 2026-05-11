using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBACWeb.Application.Services.Authorization;
using RBACWeb.Contracts.Dtos.Authorization;

namespace RBACWeb.Web.Controllers;

/// <summary>
/// Handles permission management operations.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PermissionController : BaseApiController
{
    private readonly PermissionService _permissionService;
    private readonly ILogger<PermissionController> _logger;

    public PermissionController(
        InfrastructureTools infrastructureTools,
        PermissionService permissionService,
        ILogger<PermissionController> logger) : base(infrastructureTools)
    {
        _permissionService = permissionService;
        _logger = logger;
        ModuleCode = ModuleCodes.AuthorizationModule;
    }

    /// <summary>
    /// Retrieves all permissions.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PermissionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllPermissions()
    {
        _logger.LogInformation("Retrieving all permissions");
        var result = await _permissionService.GetAllAsync(HttpContext.RequestAborted);
        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorCode!, result.ErrorMessage);
        }
        return Ok(result.Data);
    }

    /// <summary>
    /// Retrieves a permission by ID.
    /// </summary>
    /// <param name="id">Permission ID</param>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PermissionDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPermissionById(long id)
    {
        _logger.LogInformation("Retrieving permission with ID: {PermissionId}", id);
        var result = await _permissionService.GetByIdAsync(id, HttpContext.RequestAborted);
        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorCode!, result.ErrorMessage);
        }
        return Ok(result.Data);
    }

    /// <summary>
    /// Retrieves permissions by resource ID.
    /// </summary>
    /// <param name="resourceId">Resource ID</param>
    [HttpGet("by-resource/{resourceId}")]
    [ProducesResponseType(typeof(IEnumerable<PermissionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPermissionsByResource(long resourceId)
    {
        _logger.LogInformation("Retrieving permissions for resource ID: {ResourceId}", resourceId);
        var result = await _permissionService.GetByResourceAsync(resourceId, HttpContext.RequestAborted);
        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorCode!, result.ErrorMessage);
        }
        return Ok(result.Data);
    }

    /// <summary>
    /// Creates a new permission.
    /// </summary>
    /// <param name="dto">Permission creation data</param>
    [HttpPost]
    [ProducesResponseType(typeof(PermissionDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreatePermission([FromBody] CreatePermissionDto dto)
    {
        _logger.LogInformation("Creating new permission: {PermissionCode}", dto.Code);
        var result = await _permissionService.CreateAsync(dto, HttpContext.RequestAborted);
        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorCode!, result.ErrorMessage);
        }
        return Ok(result.Data);
    }

    /// <summary>
    /// Updates an existing permission.
    /// </summary>
    /// <param name="id">Permission ID</param>
    /// <param name="dto">Permission update data</param>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(PermissionDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdatePermission(long id, [FromBody] UpdatePermissionDto dto)
    {
        _logger.LogInformation("Updating permission with ID: {PermissionId}", id);
        var result = await _permissionService.UpdateAsync(id, dto, HttpContext.RequestAborted);
        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorCode!, result.ErrorMessage);
        }
        return Ok(result.Data);
    }

    /// <summary>
    /// Deletes a permission.
    /// </summary>
    /// <param name="id">Permission ID</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeletePermission(long id)
    {
        _logger.LogInformation("Deleting permission with ID: {PermissionId}", id);
        var result = await _permissionService.DeactivateAsync(id, HttpContext.RequestAborted);
        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorCode!, result.ErrorMessage);
        }
        return Ok(true);
    }
}
