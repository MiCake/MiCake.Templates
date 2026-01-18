using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBACWeb.Application.Services.Authorization;
using RBACWeb.Contracts.Dtos.Authorization;

namespace RBACWeb.Web.Controllers;

/// <summary>
/// Handles role management operations including CRUD and permission/data scope assignments.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class RoleController : BaseApiController
{
    private readonly RoleService _roleService;
    private readonly ILogger<RoleController> _logger;

    public RoleController(
        InfrastructureTools infrastructureTools,
        RoleService roleService,
        ILogger<RoleController> logger) : base(infrastructureTools)
    {
        _roleService = roleService;
        _logger = logger;
        ModuleCode = ModuleCodes.AuthorizationModule;
    }

    /// <summary>
    /// Retrieves all roles.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RoleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllRoles()
    {
        _logger.LogInformation("Retrieving all roles");
        var result = await _roleService.GetAllAsync(HttpContext.RequestAborted);
        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorCode!, result.ErrorMessage);
        }
        return Ok(result.Data);
    }

    /// <summary>
    /// Retrieves a role by ID.
    /// </summary>
    /// <param name="id">Role ID</param>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RoleDetailDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoleById(long id)
    {
        _logger.LogInformation("Retrieving role with ID: {RoleId}", id);
        var result = await _roleService.GetByIdAsync(id, HttpContext.RequestAborted);
        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorCode!, result.ErrorMessage);
        }
        return Ok(result.Data);
    }

    /// <summary>
    /// Creates a new role.
    /// </summary>
    /// <param name="dto">Role creation data</param>
    [HttpPost]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto)
    {
        _logger.LogInformation("Creating new role: {RoleCode}", dto.Code);
        var result = await _roleService.CreateAsync(dto, HttpContext.RequestAborted);
        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorCode!, result.ErrorMessage);
        }
        return Ok(result.Data);
    }

    /// <summary>
    /// Updates an existing role.
    /// </summary>
    /// <param name="id">Role ID</param>
    /// <param name="dto">Role update data</param>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateRole(long id, [FromBody] UpdateRoleDto dto)
    {
        _logger.LogInformation("Updating role with ID: {RoleId}", id);
        var result = await _roleService.UpdateAsync(id, dto, HttpContext.RequestAborted);
        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorCode!, result.ErrorMessage);
        }
        return Ok(result.Data);
    }

    /// <summary>
    /// Deletes a role.
    /// </summary>
    /// <param name="id">Role ID</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteRole(long id)
    {
        _logger.LogInformation("Deleting role with ID: {RoleId}", id);
        var result = await _roleService.DeleteAsync(id, HttpContext.RequestAborted);
        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorCode!, result.ErrorMessage);
        }
        return Ok(true);
    }

    /// <summary>
    /// Assigns permissions to a role.
    /// </summary>
    /// <param name="id">Role ID</param>
    /// <param name="dto">Permission assignment data</param>
    [HttpPost("{id}/permissions")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AssignPermissions(long id, [FromBody] AssignPermissionsDto dto)
    {
        _logger.LogInformation("Assigning {Count} permissions to role {RoleId}", dto.PermissionIds.Count, id);
        var result = await _roleService.AssignPermissionsAsync(id, dto, HttpContext.RequestAborted);
        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorCode!, result.ErrorMessage);
        }
        return Ok(true);
    }

    /// <summary>
    /// Assigns data scopes to a role.
    /// </summary>
    /// <param name="id">Role ID</param>
    /// <param name="dto">Data scope assignment data</param>
    [HttpPost("{id}/datascopes")]
    public async Task<IActionResult> AssignDataScopes(long id, [FromBody] AssignDataScopesDto dto)
    {
        _logger.LogInformation("Assigning {Count} data scopes to role {RoleId}", dto.DataScopeIds.Count, id);
        var result = await _roleService.AssignDataScopesAsync(id, dto, HttpContext.RequestAborted);
        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorCode!, result.ErrorMessage);
        }
        return Ok(true);
    }
}
