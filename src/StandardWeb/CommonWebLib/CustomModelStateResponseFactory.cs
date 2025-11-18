using Microsoft.AspNetCore.Mvc;
using MiCake.AspNetCore.DataWrapper;
using CommonWebLib.CodeDefines;

namespace CommonWebLib
{
    /// <summary>
    /// Factory for creating custom invalid model state responses with unified ApiResponse format
    /// </summary>
    public static class CustomModelStateResponseFactory
    {
        public static IActionResult CreateResponse(ActionContext actionContext)
        {
            // Collect all validation errors
            var errors = actionContext.ModelState
                .Where(ms => ms.Value?.Errors.Count > 0)
                .SelectMany(ms => ms.Value!.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            var errorMessage = errors.FirstOrDefault() ?? "请求参数验证失败.";
            var apiResponse = new ApiResponse
            {
                Code = ErrorCodeDefinition.ValidationError,
                Message = errorMessage,
                Data = null
            };

            return new BadRequestObjectResult(apiResponse);
        }
    }
}