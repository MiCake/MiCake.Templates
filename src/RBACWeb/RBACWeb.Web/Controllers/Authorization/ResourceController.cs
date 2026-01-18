using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBACWeb.Application.Services.Authorization;
using RBACWeb.Contracts.Dtos.Authorization;

namespace RBACWeb.Web.Controllers;

/// <summary>
/// Handles resource management operations including tree structure retrieval.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ResourceController : BaseApiController
{
    private readonly ResourceService _resourceService;
    private readonly ILogger<ResourceController> _logger;

    public ResourceController(
        InfrastructureTools infrastructureTools,
        ResourceService resourceService,
        ILogger<ResourceController> logger) : base(infrastructureTools)
    {
        _resourceService = resourceService;
        _logger = logger;
        ModuleCode = ModuleCodes.AuthorizationModule;
    }

    /// <summary>
    /// Retrieves all resources.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ResourceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllResources()
    {
        _logger.LogInformation("Retrieving all resources");
        var result = await _resourceService.GetAllAsync(HttpContext.RequestAborted);
        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorCode!, result.ErrorMessage);
        }
        return Ok(result.Data);
    }

    /// <summary>
    /// Retrieves resources in tree structure.
    /// </summary>
    [HttpGet("tree")]
    [ProducesResponseType(typeof(IEnumerable<ResourceTreeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetResourceTree()
    {
        _logger.LogInformation("Retrieving resource tree");
        var result = await _resourceService.GetTreeAsync(HttpContext.RequestAborted);
        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorCode!, result.ErrorMessage);
        }
        return Ok(result.Data);
    }

    /// <summary>
    /// Retrieves a resource by ID.
    /// </summary>
    /// <param name="id">Resource ID</param>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ResourceDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetResourceById(long id)
    {
        _logger.LogInformation("Retrieving resource with ID: {ResourceId}", id);
        var result = await _resourceService.GetByIdAsync(id, HttpContext.RequestAborted);
        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorCode!, result.ErrorMessage);
        }
        return Ok(result.Data);
    }

    /// <summary>
    /// Creates a new resource.
    /// </summary>
    /// <param name="dto">Resource creation data</param>
    [HttpPost]
    [ProducesResponseType(typeof(ResourceDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateResource([FromBody] CreateResourceDto dto)
    {
        _logger.LogInformation("Creating new resource: {ResourceCode}", dto.Code);
        var result = await _resourceService.CreateAsync(dto, HttpContext.RequestAborted);
        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorCode!, result.ErrorMessage);
        }
        return Ok(result.Data);
    }

    /// <summary>
    /// Updates an existing resource.
    /// </summary>
    /// <param name="id">Resource ID</param>
    /// <param name="dto">Resource update data</param>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ResourceDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateResource(long id, [FromBody] UpdateResourceDto dto)
    {
        _logger.LogInformation("Updating resource with ID: {ResourceId}", id);
        var result = await _resourceService.UpdateAsync(id, dto, HttpContext.RequestAborted);
        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorCode!, result.ErrorMessage);
        }
        return Ok(result.Data);
    }

    /// <summary>
    /// Deletes a resource.
    /// </summary>
    /// <param name="id">Resource ID</param>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteResource(long id)
    {
        _logger.LogInformation("Deleting resource with ID: {ResourceId}", id);
        var result = await _resourceService.DeactivateAsync(id, HttpContext.RequestAborted);
        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorCode!, result.ErrorMessage);
        }
        return Ok(true);
    }
}
