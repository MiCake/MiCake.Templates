using Microsoft.AspNetCore.Mvc;
using StandardWeb.Domain.Models.Identity;
using StandardWeb.Domain.Repositories;
using StandardWeb.Web.Dtos.Identity;

namespace StandardWeb.Web.Controllers
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

        /// <summary>
        /// Initializes the user controller with required services.
        /// </summary>
        /// <param name="infrastructureTools">Shared infrastructure services</param>
        /// <param name="userRepo">Repository for user data access</param>
        /// <param name="logger">Logger for user operations</param>
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
        /// <returns>User profile data if found</returns>
        /// <response code="200">User found, returns profile data</response>
        /// <response code="400">User not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserById([FromRoute] long id)
        {
            var user = await _userRepo.FindAsync(id);
            if (user == null)
            {
                return BadRequest(ErrorCodeDefinition.NotFound, "User not found.");
            }

            var userDto = Mapper.Map<User, UserDto>(user);
            return Ok(userDto);
        }
    }
}
