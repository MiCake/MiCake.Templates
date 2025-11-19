using Microsoft.AspNetCore.Mvc;
using StandardWeb.Application.Models;
using StandardWeb.Web.Dtos.Identity;
using StandardWeb.Application.Services.Auth;

namespace StandardWeb.Web.Controllers
{
    /// <summary>
    /// Handles authentication operations including login and token refresh.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : BaseApiController
    {
        private readonly AuthService _authService;
        private readonly ILogger<AuthController> _logger;

        /// <summary>
        /// Initializes the authentication controller with required services.
        /// </summary>
        /// <param name="infrastructureTools">Shared infrastructure services</param>
        /// <param name="authService">Authentication service for login and token operations</param>
        /// <param name="logger">Logger for authentication events</param>
        public AuthController(
            InfrastructureTools infrastructureTools,
            AuthService authService,
            ILogger<AuthController> logger) : base(infrastructureTools)
        {
            _authService = authService;
            _logger = logger;

            ModuleCode = ModuleCodes.AuthModule;
        }

        /// <summary>
        /// Authenticates a user with phone number and password.
        /// Returns JWT access token and refresh token on successful authentication.
        /// </summary>
        /// <param name="request">Login credentials (phone number and password)</param>
        /// <returns>Login response with tokens and user information</returns>
        /// <response code="200">Login successful, returns tokens</response>
        /// <response code="400">Invalid credentials or validation failed</response>
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

            return Ok(Mapper.Map<UserLoginResult, LoginResponseDto>(loginResult.Data), ErrorCodeDefinition.Success);
        }

        /// <summary>
        /// Refreshes an expired access token using a valid refresh token.
        /// Returns new JWT access token and refresh token pair.
        /// </summary>
        /// <param name="request">Refresh token request</param>
        /// <returns>New tokens on successful refresh</returns>
        /// <response code="200">Token refresh successful, returns new tokens</response>
        /// <response code="400">Invalid or expired refresh token</response>
        [HttpPost("token/refresh")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            var result = await _authService.RefreshTokenAsync(request.RefreshToken);

            if (!result.IsSuccess)
            {
                return BadRequest(result.ErrorCode ?? ErrorCodeDefinition.OperationFailed, result.ErrorMessage);
            }

            return Ok(Mapper.Map<UserLoginResult, LoginResponseDto>(result.Data), ErrorCodeDefinition.Success);
        }
    }
}
