using EPR.Payment.Facade.Common.RESTServices.Payments;
using EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces;
using EPR.Payment.Facade.HealthCheck;
using EPR.Payment.Facade.Services.Payments.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Payment.Facade.Extension
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtensions
    {
        private static readonly string Ready = "ready";
        public static IServiceCollection AddDependencies(this IServiceCollection services)
        {
            // Register HTTP context accessor
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Register HTTP client factory
            services.AddHttpClient();

            // Register payments
            services.AddScoped<IPaymentsService, PaymentsService>();
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
