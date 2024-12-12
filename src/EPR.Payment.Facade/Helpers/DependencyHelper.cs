using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.HttpHandlers;
using EPR.Payment.Facade.Common.RESTServices.Payments;
using EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees.ComplianceScheme;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees.ComplianceScheme.Interfaces;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees.Producer.Interfaces;
using EPR.Payment.Facade.Common.RESTServices.ResubmissionFees.ComplianceScheme;
using EPR.Payment.Facade.Common.RESTServices.ResubmissionFees.ComplianceScheme.Interfaces;
using EPR.Payment.Facade.Common.RESTServices.ResubmissionFees.Producer;
using EPR.Payment.Facade.Common.RESTServices.ResubmissionFees.Producer.Interfaces;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Payment.Facade.Helpers
{
    [ExcludeFromCodeCoverage]
    public static class DependencyHelper
    {
        public static IServiceCollection AddFacadeDependencies(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Configure the services based on appsettings.json
            services.Configure<ServicesConfiguration>(configuration.GetSection(ServicesConfiguration.SectionName));

            // Register the authorization handler
            services.AddTransient<TokenAuthorizationHandler>();

            // Validate all configurations
            var serviceProvider = services.BuildServiceProvider();
            var servicesConfig = serviceProvider.GetRequiredService<IOptions<ServicesConfiguration>>().Value;

            //foreach (var property in typeof(ServicesConfiguration).GetProperties())
            //{
            //    var serviceConfig = property.GetValue(servicesConfig) as Service;
            //    ValidateServiceConfiguration(serviceConfig, property.Name);
            //}

            // Register HTTP services
            RegisterHttpService<IHttpPaymentServiceHealthCheckService, HttpOnlinePaymentServiceHealthCheckService>(
                services, nameof(ServicesConfiguration.PaymentService), "health");

            RegisterHttpService<IHttpOnlinePaymentsService, HttpOnlinePaymentsService>(
                services, nameof(ServicesConfiguration.PaymentService));

            RegisterHttpService<IHttpOfflinePaymentsService, HttpOfflinePaymentsService>(
                services, nameof(ServicesConfiguration.OfflinePaymentService));

            RegisterHttpService<IHttpGovPayService, HttpGovPayService>(
                services, nameof(ServicesConfiguration.GovPayService));

            RegisterHttpService<IHttpProducerFeesService, HttpProducerFeesService>(
                services, nameof(ServicesConfiguration.ProducerFeesService));

            RegisterHttpService<IHttpComplianceSchemeFeesService, HttpComplianceSchemeFeesService>(
                services, nameof(ServicesConfiguration.ComplianceSchemeFeesService));

            RegisterHttpService<IHttpProducerResubmissionFeesService, HttpProducerResubmissionFeesService>(
                services, nameof(ServicesConfiguration.ProducerFeesService));

            RegisterHttpService<IHttpComplianceSchemeResubmissionFeesService, HttpComplianceSchemeResubmissionFeesService>(
                services, nameof(ServicesConfiguration.ComplianceSchemeFeesService));

            return services;
        }

        private static void RegisterHttpService<TInterface, TImplementation>(
            IServiceCollection services, string configName, string? endPointOverride = null)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            // Validate and create service options
            var serviceOptions = CreateServiceOptions(services, configName, endPointOverride);

            // Configure HttpClient with the token authorization handler
            services.AddHttpClient<TInterface, TImplementation>(client =>
            {
                if (!string.IsNullOrWhiteSpace(serviceOptions.Value.Url))
                {
                    client.BaseAddress = new Uri(serviceOptions.Value.Url);
                }
            })
            .AddHttpMessageHandler<TokenAuthorizationHandler>();
        }

        private static IOptions<Service> CreateServiceOptions(IServiceCollection services, string configName, string? endPointOverride)
        {
            var serviceProvider = services.BuildServiceProvider();
            var servicesConfig = serviceProvider.GetRequiredService<IOptions<ServicesConfiguration>>().Value;

            var serviceConfig = (Service?)servicesConfig.GetType().GetProperty(configName)?.GetValue(servicesConfig);

            if (serviceConfig == null)
            {
                throw new InvalidOperationException($"Service configuration for {configName} is null.");
            }

            ValidateServiceConfiguration(serviceConfig, configName);

            var endPointName = endPointOverride ?? serviceConfig.EndPointName;

            Console.WriteLine($"Creating service options for {configName}: {JsonConvert.SerializeObject(serviceConfig)}");

            return Options.Create(new Service
            {
                Url = serviceConfig.Url ?? "https://default-url/", // Default value
                EndPointName = endPointName ?? "default-endpoint",
                BearerToken = serviceConfig.BearerToken,
                HttpClientName = serviceConfig.HttpClientName,
                Retries = serviceConfig.Retries ?? 3, // Default retries
                ServiceClientId = serviceConfig.ServiceClientId
            });
        }


        private static void ValidateServiceConfiguration(Service? serviceConfig, string configName)
        {
            if (string.IsNullOrWhiteSpace(serviceConfig?.Url))
            {
                throw new InvalidOperationException($"{configName} Url configuration is missing.");
            }

            if (string.IsNullOrWhiteSpace(serviceConfig?.EndPointName))
            {
                throw new InvalidOperationException($"{configName} EndPointName configuration is missing.");
            }
        }
    }
}
