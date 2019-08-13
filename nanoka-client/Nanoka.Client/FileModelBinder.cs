using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

namespace Nanoka.Client
{
    public class FileModelBinder : IModelBinder
    {
        readonly JsonSerializer _serializer;

        public FileModelBinder(JsonSerializer serializer)
        {
            _serializer = serializer;
        }

        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var fieldName = bindingContext.FieldName;
            var file      = bindingContext.HttpContext.Request.Form.Files.FirstOrDefault(f => f.Name.Equals(fieldName, StringComparison.Ordinal));

            if (file == null)
                return;

            string value;

            using (var stream = file.OpenReadStream())
            using (var reader = new StreamReader(stream))
                value = await reader.ReadToEndAsync();

            bindingContext.ModelState.SetModelValue(fieldName, new ValueProviderResult(value));

            try
            {
                using (var reader = new StringReader(value))
                {
                    var result = _serializer.Deserialize(reader, bindingContext.ModelType);

                    bindingContext.Result = ModelBindingResult.Success(result);
                }
            }
            catch (JsonException)
            {
                bindingContext.Result = ModelBindingResult.Failed();
            }
        }
    }
}