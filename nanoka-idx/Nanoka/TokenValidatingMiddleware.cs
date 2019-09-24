using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Nanoka
{
    public class TokenValidatingMiddleware
    {
        readonly RequestDelegate _next;

        public TokenValidatingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, TokenManager token)
        {
            if (!await token.IsValidAsync(context.RequestAborted))
                throw Result.StatusCode(HttpStatusCode.Unauthorized, "Token was invalidated.").Exception;

            await _next(context);
        }
    }
}