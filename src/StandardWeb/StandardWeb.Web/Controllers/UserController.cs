using Microsoft.AspNetCore.Mvc;
using StandardWeb.Domain.Models.Identity;
using StandardWeb.Domain.Repositories;
using StandardWeb.Web.Dtos.Identity;

namespace StandardWeb.Web.Controllers
{
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
