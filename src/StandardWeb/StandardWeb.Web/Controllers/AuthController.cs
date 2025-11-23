using Microsoft.AspNetCore.Mvc;
using StandardWeb.Web.Dtos.Identity;
using StandardWeb.Application.Services.Auth;
using StandardWeb.Web.Constants;
using StandardWeb.Contracts.Dtos.Identity;

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
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResultDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            _logger.LogInformation("Login attempt for phone number: {PhoneNumber}", request.PhoneNumber);

            var loginResult = await _authService.LoginAsync(request);

            if (!loginResult.IsSuccess)
            {
                return BadRequest(loginResult.ErrorCode ?? AuthErrorCodes.InvalidInput, loginResult.ErrorMessage);
            }

            return Ok(loginResult);
        }

        /// <summary>
        /// Refreshes an expired access token using a valid refresh token.
        /// Returns new JWT access token and refresh token pair.
        /// </summary>
        [HttpPost("token/refresh")]
        [ProducesResponseType(typeof(LoginResultDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            _logger.LogInformation("Token refresh attempt with refresh token: {RefreshToken}", request.RefreshToken);

            var result = await _authService.RefreshTokenAsync(request.RefreshToken);
            if (!result.IsSuccess)
            {
                return BadRequest(result.ErrorCode ?? BaseErrorCodes.InvalidOperation, result.ErrorMessage);
            }

            return Ok(result);
        }
    }
}
