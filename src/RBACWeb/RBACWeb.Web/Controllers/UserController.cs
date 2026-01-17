using Microsoft.AspNetCore.Mvc;
using RBACWeb.Contracts.Dtos.Identity;
using RBACWeb.Domain.Models.Identity;
using RBACWeb.Domain.Repositories;
using RBACWeb.Web.Dtos.Identity;

namespace RBACWeb.Web.Controllers
{
    /// <summary>
    /// Handles user management operations including profile retrieval.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : BaseApiController
    {
        private readonly IUserRepo _userRepo;
        private readonly ILogger<UserController> _logger;

        public UserController(InfrastructureTools infrastructureTools, IUserRepo userRepo, ILogger<UserController> logger) : base(infrastructureTools)
        {
            _userRepo = userRepo;
            _logger = logger;

            ModuleCode = ModuleCodes.UserManagementModule;
        }

        /// <summary>
        /// Retrieves user profile information by user ID.
        /// </summary>
        /// <param name="id">User ID to retrieve</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserById([FromRoute] long id)
        {
            _logger.LogInformation("Retrieving user profile for user ID: {UserId}", id);

            var user = await _userRepo.FindAsync(id);
            if (user is null)
            {
                return BadRequest(BaseErrorCodes.NotFound, "User not found.");
            }

            var userDto = Mapper.Map<User, UserDto>(user);
            return Ok(userDto);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetUsersByPaging(int pageIndex, int pageSize, [FromQuery] QueryUserListDto query)
        {
            _logger.LogInformation("Retrieving users for page {PageIndex} with size {PageSize}", pageIndex, pageSize);

            var pagedUsers = await _userRepo.FilterPagingQueryAsync(new PagingRequest(pageIndex, pageSize), query.GenerateFilterGroup());
            return Ok(MappingPagingDto<User, UserDto>(pagedUsers));
        }
    }
}
