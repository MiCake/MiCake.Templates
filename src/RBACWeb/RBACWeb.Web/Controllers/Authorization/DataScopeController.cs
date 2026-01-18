using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBACWeb.Application.Services.Authorization;
using RBACWeb.Contracts.Dtos.Authorization;

namespace RBACWeb.Web.Controllers;

/// <summary>
/// Handles data scope management operations.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DataScopeController : BaseApiController
{
    private readonly DataScopeService _dataScopeService;
    private readonly ILogger<DataScopeController> _logger;

    public DataScopeController(
        InfrastructureTools infrastructureTools,
        DataScopeService dataScopeService,
        ILogger<DataScopeController> logger) : base(infrastructureTools)
    {
        _dataScopeService = dataScopeService;
        _logger = logger;
        ModuleCode = ModuleCodes.AuthorizationModule;
    }

    /// <summary>
    /// Retrieves all data scopes.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DataScopeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllDataScopes()
    {
        _logger.LogInformation("Retrieving all data scopes");
        var result = await _dataScopeService.GetAllAsync(HttpContext.RequestAborted);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result.Data);
    }

    /// <summary>
    /// Retrieves a data scope by ID.
    /// </summary>
    /// <param name="id">Data scope ID</param>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DataScopeDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDataScopeById(long id)
    {
        _logger.LogInformation("Retrieving data scope with ID: {DataScopeId}", id);
        var result = await _dataScopeService.GetByIdAsync(id, HttpContext.RequestAborted);
        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorCode!, result.ErrorMessage!);
        }
        return Ok(result.Data);
    }

    /// <summary>
    /// Creates a new data scope.
    /// </summary>
    /// <param name="dto">Data scope creation data</param>
    [HttpPost]
    [ProducesResponseType(typeof(DataScopeDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateDataScope([FromBody] CreateDataScopeDto dto)
    {
        _logger.LogInformation("Creating new data scope: {DataScopeName}", dto.Name);
        var result = await _dataScopeService.CreateAsync(dto, HttpContext.RequestAborted);
        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorCode!, result.ErrorMessage!);
        }
        return CreatedAtAction(nameof(GetDataScopeById), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>
    /// Updates an existing data scope.
    /// </summary>
    /// <param name="id">Data scope ID</param>
    /// <param name="dto">Data scope update data</param>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(DataScopeDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateDataScope(long id, [FromBody] UpdateDataScopeDto dto)
    {
        _logger.LogInformation("Updating data scope with ID: {DataScopeId}", id);
        var result = await _dataScopeService.UpdateAsync(id, dto, HttpContext.RequestAborted);
        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorCode!, result.ErrorMessage!);
        }
        return Ok(result.Data);
    }

    /// <summary>
    /// Deletes a data scope.
    /// </summary>
    /// <param name="id">Data scope ID</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(bool),StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteDataScope(long id)
    {
        _logger.LogInformation("Deleting data scope with ID: {DataScopeId}", id);
        var result = await _dataScopeService.DeactivateAsync(id, HttpContext.RequestAborted);
        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorCode!, result.ErrorMessage!);
        }
        return Ok(true);
    }
}
