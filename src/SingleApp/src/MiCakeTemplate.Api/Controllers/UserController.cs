using MiCake.AspNetCore.DataWrapper;
using MiCake.Identity.Authentication.JwtToken;
using MiCakeTemplate.Api.Constants;
using MiCakeTemplate.Api.DtoModels.UserContext;
using MiCakeTemplate.Domain.AuthContext.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace MiCakeTemplate.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : AppControllerBase<TodoController>
    {
        private readonly IUserRepository _repo;

        public UserController(IUserRepository repo, ControllerInfrastructures infrastructures, ILoggerFactory loggerFactory) : base(infrastructures, loggerFactory)
        {
            _repo = repo;
        }

        [HttpPost("register")]
        [ProducesResponseType(200, Type = typeof(ApiResponse<bool>))]
        public async Task<IActionResult> Register([FromBody] CreateUserDto data)
        {
            var user = Domain.AuthContext.User.Create(data.UserName!, data.Password!);
            await _repo.AddAsync(user);

            return Ok(true);
        }

        [HttpPost("login")]
        [ProducesResponseType(200, Type = typeof(ApiResponse<LoginResultDto>))]
        public async Task<IActionResult> Login([FromBody] LoginDto data, [FromServices] IJwtAuthManager jwtAuthManager)
        {
            var user = await _repo.GetByUsername(data.UserName!);

            if (user is null)
            {
                return BadRequest(UserErrorCodes.NOFOUND_USER);
            }

            var (result, error) = user.LoginByPassword(data.Password!);

            if (!result)
            {
                return BadRequest(error);
            }

            // create jwt token.
            var tokenInfo = await jwtAuthManager.CreateToken(user);

            return Ok(new LoginResultDto { AccessToken = tokenInfo.AccessToken, RefreshToken = tokenInfo.RefreshToken });
        }
    }
}
