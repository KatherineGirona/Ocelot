using Microsoft.AspNetCore.Builder;

namespace Ocelot.Library.Middleware
{
    public static class AuthenticationMiddlewareMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthenticationMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthenticationMiddleware>();
        }
    }
}