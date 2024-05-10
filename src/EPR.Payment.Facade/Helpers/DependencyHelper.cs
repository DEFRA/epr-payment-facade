using EPR.Payment.Facade.Common.RESTServices;
using EPR.Payment.Facade.Common.RESTServices.Interfaces;
using EPR.Payment.Facade.Configuration;
using Microsoft.Extensions.Options;
using EPR.Payment.Facade.Services;
using EPR.Payment.Facade.Services.Interfaces;

namespace EPR.Payment.Facade.Helpers
{
    public static class ExtensionMethods
    {
        public static IServiceCollection AddFacadeDependencies(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services
                .Configure<ServicesConfiguration>(configuration.GetSection(ServicesConfiguration.SectionName));

            services
                .AddScoped<IPaymentServiceHealthService, PaymentServiceHealthService>()
                .AddScoped<IHttpPaymentServiceHealthCheckService>(s =>
                    new HttpPaymentServiceHealthCheckService(
                        s.GetRequiredService<IHttpContextAccessor>(),
                        s.GetRequiredService<IHttpClientFactory>(),
                        s.GetRequiredService<IOptions<ServicesConfiguration>>().Value.PaymentServiceAPI.Url,
                        "health"
                    )
            );
            return services;

        }
    }
}
