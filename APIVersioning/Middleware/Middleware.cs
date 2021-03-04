using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace APIVersioning.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class Middleware
    {
        private readonly RequestDelegate _next;

        public Middleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext httpContext)
        {
            if (httpContext.Request.Path.StartsWithSegments("/api") && !ContainsVersioningComponent(httpContext.Request))
            {
                var clientType = ClientType(httpContext.Request.Headers);

                httpContext.Request.Path = RewritePath(clientType, httpContext.Request.Headers);
            }
            return _next(httpContext);
        }

        private string ClientType(IHeaderDictionary headers)
        {
            return "MOBILE";
        }

        private PathString RewritePath(string clientType, IHeaderDictionary headers)
        {
            if (clientType == "MOBILE")
            {
                var appVersion = ExtractAppVersion(headers);
                if (appVersion >= 2)
                {
                    return "/api/v2/WeatherForecast";
                }
            }
            return "/api/v1/WeatherForecast";
        }

        private bool ContainsVersioningComponent(HttpRequest request)
        {
            var path = request.Path;
            var v1 = path.StartsWithSegments(new PathString("/api/v1"), StringComparison.OrdinalIgnoreCase);
            var v2 = path.StartsWithSegments(new PathString("/api/v2"), StringComparison.OrdinalIgnoreCase);

            return v1 || v2;
        }

        private int? ExtractAppVersion(IHeaderDictionary headers)
        {
            return 4;
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<Middleware>();
        }
    }
}
