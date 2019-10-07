using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Nanoka
{
    public class TokenValidatingMiddleware
    {
        readonly RequestDelegate _next;

        public TokenValidatingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        // ReSharper disable once UnusedMember.Global
        public async Task InvokeAsync(HttpContext context, TokenManager token)
        {
            if (await token.IsValidAsync(context.RequestAborted))
            {
                await _next(context);
                return;
            }

            await context.ExecuteResultAsync(new ObjectResult("Token was invalidated.")
            {
                StatusCode = (int) HttpStatusCode.Unauthorized
            });
        }
    }
}