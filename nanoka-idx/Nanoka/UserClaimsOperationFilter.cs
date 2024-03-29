using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
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
            var allowAnonymous = context.ApiDescription.CustomAttributes().OfType<AllowAnonymousAttribute>().Any();

            if (!allowAnonymous)
                operation.Security.Add(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Id   = "Bearer",
                                Type = ReferenceType.SecurityScheme
                            }
                        },
                        new List<string>()
                    }
                });

            var attr = context.MethodInfo.GetCustomAttribute<UserClaimsAttribute>(true);

            if (attr == null)
                return;

            if (attr.Permissions != UserPermissions.None)
            {
                var array = new OpenApiArray();
                array.AddRange(attr.PermissionFlags.Select(p => new OpenApiString(p.ToString())));

                operation.Extensions["x-permissions"] = array;
            }

            if (attr.Reputation > 0)
                operation.Extensions["x-reputation"] = new OpenApiDouble(attr.Reputation);

            operation.Extensions["x-unrestricted"] = new OpenApiBoolean(attr.Unrestricted);

            operation.Parameters.Add(new OpenApiParameter
            {
                Name            = "reason",
                In              = ParameterLocation.Query,
                Required        = attr.Reason,
                AllowEmptyValue = false,
                Description     = "Reason for performing this action.",
                Schema = new OpenApiSchema
                {
                    Type      = "string",
                    MinLength = UserClaimsAttribute.MinReasonLength
                }
            });
        }
    }
}