using EPR.Payment.Facade.Services.Interfaces;
using EPR.Payment.Facade.Services;
using System.Diagnostics.CodeAnalysis;
using EPR.Payment.Facade.Common.RESTServices.Interfaces;
using EPR.Payment.Facade.Common.RESTServices;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using EPR.Payment.Facade.HealthCheck;

namespace EPR.Payment.Facade.Extension
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtensions
    {
        private static readonly string Ready = "ready";
        public static IServiceCollection AddDependencies(this IServiceCollection services)
        {
            services.AddScoped<IFeesService, FeesService>();
            services.AddScoped<IPaymentsService, PaymentsService>();
            services.AddScoped<IHttpFeesService, HttpFeesService>();
            services.AddScoped<IHttpGovPayService, HttpGovPayService>();
            services.AddScoped<IHttpPaymentsService, HttpPaymentsService>();
            services.AddHttpContextAccessor();
            services.AddHttpClient();

            return services;

        }

        public static IServiceCollection AddServiceHealthChecks(this IServiceCollection services) 
        {
            services.AddHealthChecks()
                .AddCheck<PaymentsFacadeHealthCheck>(PaymentsFacadeHealthCheck.HealthCheckResultDescription,
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new[] { Ready });
            return services;
        }
    }
}
