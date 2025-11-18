using Microsoft.AspNetCore.Mvc;
using StandardWeb.Application.Models;
using StandardWeb.Web.Dtos.Identity;
using StandardWeb.Application.Services.Auth;

namespace StandardWeb.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : BaseApiController
    {
        private readonly AuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            InfrastructureTools infrastructureTools,
            AuthService authService,
            ILogger<AuthController> logger) : base(infrastructureTools)
        {
            _authService = authService;
            _logger = logger;

            ModuleCode = ModuleCodes.AuthModule;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var loginResult = await _authService.LoginAsync(new UserLoginModel
            {
                PhoneNumber = request.PhoneNumber,
                Password = request.Password
            });

            if (!loginResult.IsSuccess)
            {
                return BadRequest(loginResult.ErrorCode ?? ErrorCodeDefinition.OperationFailed, loginResult.ErrorMessage);
            }

            return Ok(Mapper.Map<UserLoginResult, LoginResponseDto>(loginResult.Data));
        }

        [HttpPost("token/refresh")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            var result = await _authService.RefreshTokenAsync(request.RefreshToken);

            if (!result.IsSuccess)
            {
                return BadRequest(result.ErrorCode ?? ErrorCodeDefinition.OperationFailed, result.ErrorMessage);
            }

            return Ok(Mapper.Map<UserLoginResult, LoginResponseDto>(result.Data));
        }
    }
}
