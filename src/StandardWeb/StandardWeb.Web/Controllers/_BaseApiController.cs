using AutoMapper;
using MiCake.AspNetCore.Responses;
using MiCake.Core.DependencyInjection;
using MiCake.Util.Query.Paging;
using Microsoft.AspNetCore.Mvc;
using StandardWeb.Common.Helpers;

namespace CommonWebLib
{
    /// <summary>
    /// Base controller providing common API functionality and standardized responses.
    /// All API controllers should inherit from this class to maintain consistency.
    /// </summary>
    public class BaseApiController : ControllerBase
    {
        /// <summary>
        /// Two-digit module identifier used to prefix error codes (e.g., "01" for auth module).
        /// Override in derived controllers to categorize errors by module.
        /// </summary>
        public string ModuleCode { get; protected set; } = "00"; // Default module code

        /// <summary>
        /// Cancellation token for the current HTTP request.
        /// Use this to cancel long-running operations when the client disconnects.
        /// </summary>
        public CancellationToken HttpCancellationToken => HttpContext.RequestAborted;

        /// <summary>
        /// Logger instance for the base controller.
        /// </summary>
        protected ILogger<BaseApiController> BaseLogger { get; }

        /// <summary>
        /// Initializes the base controller with required infrastructure services.
        /// </summary>
        /// <param name="infrastructureTools">Shared infrastructure services (mapper, logging)</param>
        public BaseApiController(InfrastructureTools infrastructureTools)
        {
            InfrastructureTools = infrastructureTools;
            BaseLogger = infrastructureTools.LoggerFactory.CreateLogger<BaseApiController>();
        }

        private InfrastructureTools InfrastructureTools { get; }

        /// <summary>
        /// AutoMapper instance for DTO conversions.
        /// </summary>
        public IMapper Mapper => InfrastructureTools.Mapper;

        /// <summary>
        /// Returns a standardized 400 Bad Request response with module-prefixed error code.
        /// </summary>
        /// <param name="code">Error code (will be prefixed with ModuleCode)</param>
        /// <param name="message">Human-readable error message</param>
        /// <param name="data">Optional additional error data</param>
        protected IActionResult BadRequest(string code, string? message, object? data = null)
        {
            return base.BadRequest(new ApiResponse
            {
                Code = $"{ModuleCode}.{code}",
                Message = message,
                Data = data!
            });
        }

        /// <summary>
        /// Returns a standardized 401 Unauthorized response with module-prefixed error code.
        /// </summary>
        /// <param name="code">Error code (will be prefixed with ModuleCode)</param>
        /// <param name="message">Human-readable error message (defaults to "Unauthorized access.")</param>
        /// <param name="data">Optional additional error data</param>
        protected IActionResult Unauthorized(string code, string? message = null, object? data = null)
        {
            return base.Unauthorized(new ApiResponse
            {
                Code = $"{ModuleCode}.{code}",
                Message = message ?? "Unauthorized access.",
                Data = data!
            });
        }

        /// <summary>
        /// Returns a standardized 200 OK response with module-prefixed success code.
        /// </summary>
        /// <param name="data">Response data</param>
        /// <param name="code">Success code (will be prefixed with ModuleCode)</param>
        /// <param name="message">Human-readable success message (defaults to "Operation successful.")</param>
        protected IActionResult Ok(object? data, string code, string? message = null)
        {
            return base.Ok(new ApiResponse
            {
                Code = $"{ModuleCode}.{code}",
                Message = message ?? "Operation successful.",
                Data = data!
            });
        }

        /// <summary>
        /// Extracts the current user ID from JWT claims.
        /// </summary>
        /// <param name="showErrorWhenNull">Whether to throw exception if user ID is not found</param>
        /// <returns>User ID from token claims, or null if not authenticated</returns>
        /// <exception cref="InvalidOperationException">Thrown when user ID is null and showErrorWhenNull is true</exception>
        protected long? GetCurrentUserId(bool showErrorWhenNull = true)
        {
            var userId = TokenClaimHelper.GetUserIdFromClaims(User);
            if (userId == null && showErrorWhenNull)
            {
                throw new InvalidOperationException("User ID cannot be null. Ensure the user is authenticated.");
            }
            return userId;
        }

        /// <summary>
        /// Maps a paging result from domain models to DTOs while preserving pagination metadata.
        /// </summary>
        /// <typeparam name="T">Source domain model type</typeparam>
        /// <typeparam name="TDto">Target DTO type</typeparam>
        /// <param name="pagingResult">Paging result containing domain models</param>
        /// <returns>Paging result containing mapped DTOs, or null if input is null</returns>
        protected PagingResponse<TDto>? MappingPagingDto<T, TDto>(PagingResponse<T> pagingResult)
            where TDto : class
        {
            if (pagingResult == null)
            {
                return null;
            }

            var dtoList = Mapper.Map<List<TDto>>(pagingResult.Data);
            return new PagingResponse<TDto>(pagingResult.CurrentIndex, pagingResult.TotalCount, dtoList);
        }
    }

    /// <summary>
    /// Container for shared infrastructure services used across controllers.
    /// Registered as a scoped service in DI container.
    /// </summary>
    [InjectService(typeof(InfrastructureTools))]
    public class InfrastructureTools(IMapper mapper, ILoggerFactory loggerFactory)
    {
        /// <summary>
        /// AutoMapper instance for object-to-object mapping.
        /// </summary>
        public IMapper Mapper { get; set; } = mapper;

        /// <summary>
        /// Factory for creating logger instances.
        /// </summary>
        public ILoggerFactory LoggerFactory { get; set; } = loggerFactory;
    }
}
