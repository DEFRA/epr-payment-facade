using EPR.Payment.Facade.HealthCheck;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Payment.Facade.AppStart
{
    [ExcludeFromCodeCoverage]
    public static class HealthCheckStartup
    {
        public static IApplicationBuilder UseHealthChecks(this IApplicationBuilder app)
        {
            app.UseHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = HealthCheckResponseWriter.WriteJsonResponse
            });

            app.UseHealthChecks("/ping", new HealthCheckOptions
            {
                Predicate = (_) => false,
                ResponseWriter = (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync("");
                }
            });

            return app;
        }
    }
}