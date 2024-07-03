using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.RESTServices.Payments;
using EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces;
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

            RegisterHttpService<IHttpPaymentServiceHealthCheckService, HttpPaymentServiceHealthCheckService>(
                services, nameof(ServicesConfiguration.PaymentService), "health");

            RegisterHttpService<IHttpPaymentsService, HttpPaymentsService>(
                services, nameof(ServicesConfiguration.PaymentService));

            RegisterHttpService<IHttpGovPayService, HttpGovPayService>(
                services, nameof(ServicesConfiguration.GovPayService));

            return services;
        }

        private static void RegisterHttpService<TInterface, TImplementation>(
            IServiceCollection services, string configName, string endPointOverride = null)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            services.AddScoped<TInterface>(s =>
            {
                var servicesConfig = s.GetRequiredService<IOptions<ServicesConfiguration>>().Value;
                var serviceConfig = (Service)servicesConfig.GetType().GetProperty(configName)?.GetValue(servicesConfig);

                if (serviceConfig?.Url == null)
                {
                    throw new InvalidOperationException($"{configName} Url configuration is missing.");
                }

                var endPointName = endPointOverride ?? serviceConfig?.EndPointName;

                if (endPointName == null)
                {
                    throw new InvalidOperationException($"{configName} EndPointName configuration is missing.");
                }

                var serviceOptions = Options.Create(new Service
                {
                    Url = serviceConfig.Url,
                    EndPointName = endPointName,
                    BearerToken = serviceConfig.BearerToken,
                    HttpClientName = serviceConfig.HttpClientName
                });

                return (TImplementation)Activator.CreateInstance(typeof(TImplementation),
                    s.GetRequiredService<IHttpContextAccessor>(),
                    s.GetRequiredService<IHttpClientFactory>(),
                    serviceOptions) ?? throw new InvalidOperationException($"Failed to create instance of {typeof(TImplementation).Name}");
            });
        }
    }
}
