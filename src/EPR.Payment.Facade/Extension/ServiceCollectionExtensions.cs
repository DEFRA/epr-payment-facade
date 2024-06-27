using EPR.Payment.Facade.Services.Interfaces;
using EPR.Payment.Facade.Services;
using System.Diagnostics.CodeAnalysis;
using EPR.Payment.Facade.Common.RESTServices.Interfaces;
using EPR.Payment.Facade.Common.RESTServices;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using EPR.Payment.Facade.HealthCheck;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EPR.Payment.Facade.Extension
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtensions
    {
        private static readonly string Ready = "ready";

        public static IServiceCollection AddDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            var baseUrl = configuration.GetValue<string>("EPRPaymentService:BaseUrl");
            services.AddSingleton<IHttpFeesService>(provider =>
            {
                var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
                var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
                return new HttpFeesService(httpContextAccessor, httpClientFactory, baseUrl);
            });

            services.AddTransient<IFeesService, FeesService>();
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
