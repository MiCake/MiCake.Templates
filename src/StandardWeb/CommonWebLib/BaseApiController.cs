using AutoMapper;
using MiCake.AspNetCore.DataWrapper;
using MiCake.Core.DependencyInjection;
using MiCake.DDD.Extensions.Paging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StandardWeb.Common.Helpers;

namespace CommonWebLib
{
    public class BaseApiController : ControllerBase
    {
        public string ModuleCode { get; protected set; } = "00"; // Default module code

        public CancellationToken HttpCancellationToken => HttpContext.RequestAborted;
        protected ILogger<BaseApiController> BaseLogger { get; }

        public BaseApiController(InfrastructureTools infrastructureTools)
        {
            InfrastructureTools = infrastructureTools;
            BaseLogger = infrastructureTools.LoggerFactory.CreateLogger<BaseApiController>();
        }

        private InfrastructureTools InfrastructureTools { get; }

        public IMapper Mapper => InfrastructureTools.Mapper;

        protected IActionResult BadRequest(string code, string? message, object? data = null)
        {
            return base.BadRequest(new ApiResponse
            {
                Code = $"{ModuleCode}.{code}",
                Message = message,
                Data = data
            });
        }

        protected IActionResult Unauthorized(string code, string? message = null, object? data = null)
        {
            return base.Unauthorized(new ApiResponse
            {
                Code = $"{ModuleCode}.{code}",
                Message = message ?? "Unauthorized access.",
                Data = data
            });
        }

        protected IActionResult Ok(object? data, string code, string? message = null)
        {
            return base.Ok(new ApiResponse
            {
                Code = $"{ModuleCode}.{code}",
                Message = message ?? "Operation successful.",
                Data = data
            });
        }

        protected long? GetCurrentUserId(bool showErrorWhenNull = true)
        {
            var userId = TokenClaimHelper.GetUserIdFromClaims(User);
            if (userId == null && showErrorWhenNull)
            {
                throw new InvalidOperationException("User ID cannot be null. Ensure the user is authenticated.");
            }
            return userId;
        }

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

    [InjectService(typeof(InfrastructureTools))]
    public class InfrastructureTools(IMapper mapper, ILoggerFactory loggerFactory)
    {
        public IMapper Mapper { get; set; } = mapper;

        public ILoggerFactory LoggerFactory { get; set; } = loggerFactory;
    }
}
