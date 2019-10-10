using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Nanoka
{
    public class VerifyHumanOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var attr = context.MethodInfo.GetCustomAttribute<VerifyHumanAttribute>(true);

            if (attr == null)
                return;

            operation.Parameters.Add(new OpenApiParameter
            {
                Name            = "recaptcha",
                In              = ParameterLocation.Query,
                Required        = true,
                AllowEmptyValue = false,
                Description     = "reCAPTCHA token.",
                Schema = new OpenApiSchema
                {
                    Type = "string"
                }
            });
        }
    }
}