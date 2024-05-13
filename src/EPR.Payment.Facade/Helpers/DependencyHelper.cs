using EPR.Payment.Facade.Common.RESTServices;
using EPR.Payment.Facade.Common.RESTServices.Interfaces;
using EPR.Payment.Facade.Configuration;
using EPR.Payment.Facade.Services.Interfaces;
using EPR.Payment.Facade.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

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
