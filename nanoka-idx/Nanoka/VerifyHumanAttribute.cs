using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Nanoka
{
    public class VerifyHumanAttribute : TypeFilterAttribute
    {
        public VerifyHumanAttribute() : base(typeof(Filter)) { }

        sealed class Filter : IAsyncAuthorizationFilter
        {
            public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
            {
                var validator = context.HttpContext.RequestServices.GetService<RecaptchaValidator>();

                await validator.ValidateAsync(context.HttpContext.Request.Query["recaptcha"], context.HttpContext.RequestAborted);
            }
        }
    }
}
