using Microsoft.AspNetCore.Builder;
using Ocelot.DownstreamUrlCreator.Middleware;
using Ocelot.Middleware.Pipeline;

namespace Ocelot.Request.Middleware
{
    public static class HttpRequestBuilderMiddlewareExtensions
    {
        public static IOcelotPipelineBuilder UseDownstreamRequestInitialiser(this IOcelotPipelineBuilder builder)
        {
            return builder.UseMiddleware<DownstreamRequestInitialiserMiddleware>()
                .UseMiddleware<DownstreamMethodTransformerMiddleware>();
        }
    }
}
