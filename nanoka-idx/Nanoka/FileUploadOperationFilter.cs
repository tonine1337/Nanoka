using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Nanoka
{
    /// <summary>
    /// https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/193#issuecomment-254878145
    /// </summary>
    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var desc = context.ApiDescription.ParameterDescriptions.First(p => p.ModelMetadata.ContainerType == typeof(IFormFile));

            //operation.Parameters.Remove(operation.Parameters.First(p => p.Name == desc.ModelMetadata.Name));

            var consumes = context.ApiDescription
                                  .CustomAttributes()
                                  .OfType<ConsumesAttribute>()
                                  .FirstOrDefault()
                                 ?.ContentTypes
                        ?? new MediaTypeCollection { "application/octet-stream" };

            operation.RequestBody = new OpenApiRequestBody
            {
                Content = consumes.ToDictionary(
                    x => x,
                    x => new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            // https://swagger.io/docs/specification/data-models/data-types/#file
                            Type   = "string",
                            Format = "file"
                        }
                    }),
                Required    = true,
                Description = desc.ModelMetadata.Description
            };
        }
    }
}