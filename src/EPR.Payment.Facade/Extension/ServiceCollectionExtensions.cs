using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using EPR.Payment.Facade.HealthCheck;
using EPR.Payment.Facade.Services.Fees.Registration;
using EPR.Payment.Facade.Services.Fees.Registration.Interfaces;
using EPR.Payment.Facade.Services.Payments.Interfaces;
using EPR.Payment.Facade.Services.Payments;
using EPR.Payment.Facade.Services.Fees;
using EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces;
using EPR.Payment.Facade.Common.RESTServices.Payments;

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

            // Register ComplianceSchemeHttpService
            services.AddScoped(sp =>
            {
                var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var baseUrl = "https://external-api-base-url"; // Replace with actual base URL
                var endPointName = "fees"; // Replace with actual endpoint name

                return new ComplianceSchemeHttpService(httpContextAccessor, httpClientFactory, baseUrl, endPointName);
            });

            // Register LargeProducerHttpService
            services.AddScoped(sp =>
            {
                var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var baseUrl = "https://external-api-base-url"; // Replace with actual base URL
                var endPointName = "largeproducer"; // Replace with actual endpoint name

                return new LargeProducerHttpService(httpContextAccessor, httpClientFactory, baseUrl, endPointName);
            });

            // Register SmallProducerHttpService
            services.AddScoped(sp =>
            {
                var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var baseUrl = "https://external-api-base-url"; // Replace with actual base URL
                var endPointName = "smallproducer"; // Replace with actual endpoint name

                return new SmallProducerHttpService(httpContextAccessor, httpClientFactory, baseUrl, endPointName);
            });

            // Register fee calculator services
            services.AddScoped<IFeeCalculatorService<RegistrationFeeRequest>, ComplianceSchemeFeeCalculatorService>();
            services.AddScoped<IFeeCalculatorService<RegistrationFeeRequest>, LargeProducerFeeCalculatorService>();
            services.AddScoped<IFeeCalculatorService<RegistrationFeeRequest>, SmallProducerFeeCalculatorService>();

            // Register factory
            services.AddScoped<IFeeCalculatorFactory, FeeCalculatorFactory>();

            // Regiser payments
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
