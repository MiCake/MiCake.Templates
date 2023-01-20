using MiCake.AspNetCore.DataWrapper;
using MiCakeTemplate.Util.Common.Constants;

namespace MiCakeTemplate.Api.Middlewares
{
    public class AppExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AppExceptionHandlerMiddleware> _logger;

        public AppExceptionHandlerMiddleware(RequestDelegate next, ILogger<AppExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public virtual async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception e)
            {
                _logger.LogError(exception: e, message: "{msg}", e.Message);

                await HandleExceptionAsync(httpContext, e);
            }
        }

        public static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            return context.Response.WriteAsJsonAsync(ApiResponse.Failure("An internal application error: " + exception.Message, CommonErrorCodes.System_ERROR.Code));
        }
    }
}
