using MiCake.AspNetCore.DataWrapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace MiCakeTemplate.Api.Extentsions
{
    /* Used to wrap all ok(http code 200) result to 'ApiResponse' structure.
     * 
     * So you can write your code like this :   
     * [ProducesResponseType(200, Type = typeof(MySampleDto))] 
     * 
     * This is equivalent to :
     *  [ProducesResponseType(200, Type = typeof(ApiResponse<MySampleDto>))]
     */


    public class SwaggerApiResponseFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            //Fixing the swagger response for Controller style endpoints
            if (context.ApiDescription.ActionDescriptor is not ControllerActionDescriptor cad)
            {
                return;
            }

            // only process 200 status code.
            // you can also custom your http status code process logic.
            var successResTag = context.MethodInfo.GetCustomAttributes<ProducesResponseTypeAttribute>().FirstOrDefault(s => s.StatusCode == 200);
            if (successResTag is null)
            {
                return;
            }

            var returnType = successResTag.Type;
            if (typeof(IWrappedResponse).IsAssignableFrom(returnType))
            {
                return;
            }

            // Wrap all response type to ApiResponse.
            var actualType = typeof(ApiResponse<>).MakeGenericType(returnType);
            var schema = context.SchemaGenerator.GenerateSchema(actualType, context.SchemaRepository);
            foreach (var item in operation.Responses["200"].Content)
            {
                item.Value.Schema = schema;
            }
        }
    }
}
