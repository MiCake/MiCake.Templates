using AutoMapper;
using MiCake.AspNetCore.DataWrapper;
using MiCake.Core.DependencyInjection;
using MiCake.DDD.Extensions.Paging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StandardWeb.Common.Helpers;

namespace CommonWebLib
{
    /// <summary>
    /// Base controller providing common functionality for API controllers including
    /// standardized response formatting, user context, and AutoMapper integration
    /// </summary>
    /// <summary>
    /// Base controller providing common functionality for API controllers including
    /// standardized response formatting, user context, and AutoMapper integration
    /// </summary>
    public class BaseApiController : ControllerBase
    {
        /// <summary>
        /// Gets or sets the module code used for error code prefixing
        /// </summary>
        public string ModuleCode { get; protected set; } = "00";

        /// <summary>
        /// Gets the cancellation token for the current HTTP request
        /// </summary>
        public CancellationToken HttpCancellationToken => HttpContext.RequestAborted;
        
        protected ILogger<BaseApiController> BaseLogger { get; }

        public BaseApiController(InfrastructureTools infrastructureTools)
        {
            InfrastructureTools = infrastructureTools;
            BaseLogger = infrastructureTools.LoggerFactory.CreateLogger<BaseApiController>();
        }

        private InfrastructureTools InfrastructureTools { get; }

        /// <summary>
        /// Gets the AutoMapper instance for object mapping
        /// </summary>
        public IMapper Mapper => InfrastructureTools.Mapper;

        /// <summary>
        /// Creates a standardized bad request response with error details
        /// </summary>
        /// <param name="code">Error code (will be prefixed with module code)</param>
        /// <param name="message">Error message</param>
        /// <param name="data">Optional additional data</param>
        /// <returns>BadRequest action result with standardized error format</returns>
        protected IActionResult BadRequest(string code, string? message, object? data = null)
        {
            return base.BadRequest(new ApiResponse
            {
                Code = $"{ModuleCode}.{code}",
                Message = message,
                Data = data
            });
        }

        /// <summary>
        /// Creates a standardized unauthorized response with error details
        /// </summary>
        /// <param name="code">Error code (will be prefixed with module code)</param>
        /// <param name="message">Error message (defaults to "Unauthorized access.")</param>
        /// <param name="data">Optional additional data</param>
        /// <returns>Unauthorized action result with standardized error format</returns>
        protected IActionResult Unauthorized(string code, string? message = null, object? data = null)
        {
            return base.Unauthorized(new ApiResponse
            {
                Code = $"{ModuleCode}.{code}",
                Message = message ?? "Unauthorized access.",
                Data = data
            });
        }

        /// <summary>
        /// Creates a standardized success response with data
        /// </summary>
        /// <param name="data">Response data</param>
        /// <param name="code">Success code (will be prefixed with module code)</param>
        /// <param name="message">Success message (defaults to "Operation successful.")</param>
        /// <returns>OK action result with standardized success format</returns>
        protected IActionResult Ok(object? data, string code, string? message = null)
        {
            return base.Ok(new ApiResponse
            {
                Code = $"{ModuleCode}.{code}",
                Message = message ?? "Operation successful.",
                Data = data
            });
        }

        /// <summary>
        /// Retrieves the current user's ID from JWT claims
        /// </summary>
        /// <param name="showErrorWhenNull">Whether to throw an exception if user ID is not found</param>
        /// <returns>The current user's ID if authenticated; otherwise null or throws exception based on parameter</returns>
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
        /// Maps a paged result of domain entities to a paged result of DTOs
        /// </summary>
        /// <typeparam name="T">Source entity type</typeparam>
        /// <typeparam name="TDto">Target DTO type</typeparam>
        /// <param name="pagingResult">The paging result to map</param>
        /// <returns>A paged result containing mapped DTOs, or null if input is null</returns>
        protected PagingQueryResult<TDto>? MappingPagingDto<T, TDto>(PagingQueryResult<T> pagingResult)
            where TDto : class
        {
            if (pagingResult == null)
            {
                return null;
            }

            var dtoList = Mapper.Map<List<TDto>>(pagingResult.Data);
            return new PagingQueryResult<TDto>(pagingResult.CurrentIndex, pagingResult.TotalCount, dtoList);
        }
    }

    /// <summary>
    /// Provides infrastructure dependencies for controllers (AutoMapper, logging, etc.)
    /// </summary>
    [InjectService(typeof(InfrastructureTools))]
    public class InfrastructureTools(IMapper mapper, ILoggerFactory loggerFactory)
    {
        public IMapper Mapper { get; set; } = mapper;

        public ILoggerFactory LoggerFactory { get; set; } = loggerFactory;
    }
}
