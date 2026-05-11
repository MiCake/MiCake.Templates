using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBACWeb.Application.Services.Authorization;
using RBACWeb.Contracts.Dtos.Authorization;

namespace RBACWeb.Web.Controllers;

/// <summary>
/// Handles user role assignment operations.
/// </summary>
[Route("api/users/{userId}/roles")]
[ApiController]
[Authorize]
public class UserRoleController : BaseApiController
{
    private readonly UserRoleService _userRoleService;
    private readonly ILogger<UserRoleController> _logger;

    public UserRoleController(
        InfrastructureTools infrastructureTools,
        UserRoleService userRoleService,
        ILogger<UserRoleController> logger) : base(infrastructureTools)
    {
        _userRoleService = userRoleService;
        _logger = logger;
        ModuleCode = ModuleCodes.AuthorizationModule;
    }

    /// <summary>
    /// Retrieves all roles assigned to a user.
    /// </summary>
    /// <param name="userId">User ID</param>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserRoleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserRoles(long userId)
    {
        _logger.LogInformation("Retrieving roles for user {UserId}", userId);
        var result = await _userRoleService.GetUserRolesAsync(userId, HttpContext.RequestAborted);
        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorCode!, result.ErrorMessage);
        }
        return Ok(result.Data);
    }

    /// <summary>
    /// Retrieves all permissions for a user.
    /// </summary>
    /// <param name="userId">User ID</param>
    [HttpGet("permissions")]
    [ProducesResponseType(typeof(UserPermissionsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserPermissions(long userId)
    {
        _logger.LogInformation("Retrieving permissions for user {UserId}", userId);
        var result = await _userRoleService.GetUserPermissionsAsync(userId, HttpContext.RequestAborted);
        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorCode!, result.ErrorMessage);
        }
        return Ok(result.Data);
    }

    /// <summary>
    /// Assigns a role to a user.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="dto">Role assignment data</param>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AssignRole(long userId, [FromBody] AssignUserRoleDto dto)
    {
        _logger.LogInformation("Assigning role {RoleId} to user {UserId}", dto.RoleId, userId);
        var result = await _userRoleService.AssignRoleAsync(userId, dto, HttpContext.RequestAborted);
        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorCode!, result.ErrorMessage);
        }
        return Ok(true);
    }

    /// <summary>
    /// Removes a role from a user.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="roleId">Role ID to remove</param>
    [HttpDelete("{roleId}")]
    public async Task<IActionResult> RemoveRole(long userId, long roleId)
    {
        _logger.LogInformation("Removing role {RoleId} from user {UserId}", roleId, userId);
        var result = await _userRoleService.RemoveRoleAsync(userId, roleId, HttpContext.RequestAborted);
        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorCode!, result.ErrorMessage);
        }
        return Ok(true);
    }
}
