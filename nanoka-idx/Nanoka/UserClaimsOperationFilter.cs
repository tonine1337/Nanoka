using System.Linq;
using System.Reflection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Nanoka.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Nanoka
{
    public class UserClaimsOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var attr = context.MethodInfo.GetCustomAttribute<UserClaimsAttribute>(true);

            if (attr == null)
                return;

            if (attr.Permissions != UserPermissions.None)
            {
                var array = new OpenApiArray();
                array.AddRange(attr.PermissionFlags.Select(p => new OpenApiString(p.ToString())));

                operation.Extensions["permissions"] = array;
            }

            if (attr.Reputation > 0)
                operation.Extensions["reputation"] = new OpenApiDouble(attr.Reputation);

            operation.Extensions["unrestricted"] = new OpenApiBoolean(attr.Unrestricted);

            operation.Parameters.Add(new OpenApiParameter
            {
                Name            = "reason",
                In              = ParameterLocation.Query,
                Required        = attr.Reason,
                AllowEmptyValue = false,
                Description     = "Reason for performing this action."
            });
        }
    }
}