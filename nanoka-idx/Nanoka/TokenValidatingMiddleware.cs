using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Nanoka
{
    public class TokenValidatingMiddleware : IMiddleware
    {
        readonly TokenManager _tokens;

        public TokenValidatingMiddleware(TokenManager tokens)
        {
            _tokens = tokens;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (!await _tokens.IsValidAsync(context.RequestAborted))
                throw Result.StatusCode(HttpStatusCode.Unauthorized, "Token was invalidated.").Exception;

            await next(context);
        }
    }
}