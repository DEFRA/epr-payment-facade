using EPR.Payment.Facade.Common.RESTServices.Payments;
using EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces;
using EPR.Payment.Facade.Configuration;
using EPR.Payment.Facade.Services.Payments;
using EPR.Payment.Facade.Services.Payments.Interfaces;
using Microsoft.Extensions.Options;

namespace EPR.Payment.Facade.Helpers
{
    public static class ExtensionMethods
    {
        public static IServiceCollection AddFacadeDependencies(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<ServicesConfiguration>(configuration.GetSection(ServicesConfiguration.SectionName));

            services.AddScoped<IPaymentServiceHealthService, PaymentServiceHealthService>();

            services.AddScoped<IHttpPaymentServiceHealthCheckService>(s =>
            {
                var baseUrl = s.GetRequiredService<IOptions<ServicesConfiguration>>().Value?.PaymentServiceAPI?.Url;

                if (baseUrl == null)
                {
                    throw new InvalidOperationException("Base URL for the payment service API is null.");
                }

                return new HttpPaymentServiceHealthCheckService(
                    s.GetRequiredService<IHttpContextAccessor>(),
                    s.GetRequiredService<IHttpClientFactory>(),
                    baseUrl,
                    "health"
                );
            });

            return services;
        }
    }
}
