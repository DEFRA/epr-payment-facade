using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.RESTServices.Payments;
using EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees.ComplianceScheme;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees.ComplianceScheme.Interfaces;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees.Producer.Interfaces;
using EPR.Payment.Facade.Common.RESTServices.ResubmissionFees.Producer;
using EPR.Payment.Facade.Common.RESTServices.ResubmissionFees.Producer.Interfaces;
using EPR.Payment.Facade.Services.Payments;
using EPR.Payment.Facade.Services.Payments.Interfaces;
using Microsoft.Extensions.Options;
using System.Diagnostics;
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
            services.Configure<ServicesConfiguration>(configuration.GetSection(ServicesConfiguration.SectionName));

            services.AddScoped<IPaymentServiceHealthService, PaymentServiceHealthService>();

            RegisterHttpService<IHttpPaymentServiceHealthCheckService, HttpPaymentServiceHealthCheckService>(
                services, nameof(ServicesConfiguration.PaymentService), "health");

            RegisterHttpService<IHttpPaymentsService, HttpPaymentsService>(
                services, nameof(ServicesConfiguration.PaymentService));

            RegisterHttpService<IHttpGovPayService, HttpGovPayService>(
            services, nameof(ServicesConfiguration.GovPayService));

            RegisterHttpService<IHttpProducerFeesService, HttpProducerFeesService>(
                services, nameof(ServicesConfiguration.ProducerFeesService));

            RegisterHttpService<IHttpComplianceSchemeFeesService, HttpComplianceSchemeFeesService>(
                services, nameof(ServicesConfiguration.ComplianceSchemeFeesService));

            RegisterHttpService<IHttpProducerResubmissionFeesService, HttpProducerResubmissionFeesService>(
                services, nameof(ServicesConfiguration.ProducerFeesService));

            return services;
        }

        private static void RegisterHttpService<TInterface, TImplementation>(
            IServiceCollection services, string configName, string? endPointOverride = null)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            // Perform validation before adding to the service collection
            var serviceOptions = CreateServiceOptions(services, configName, endPointOverride);

            services.AddScoped<TInterface>(s =>
            {
                Trace.TraceInformation($"Registering service {typeof(TImplementation).Name} for {configName}");

                var instance = Activator.CreateInstance(typeof(TImplementation),
                    s.GetRequiredService<IHttpContextAccessor>(),
                    s.GetRequiredService<IHttpClientFactory>(),
                    serviceOptions);

                Trace.TraceError(instance == null ? $"Failed to create instance of {typeof(TImplementation).Name}" : $"Successfully created instance of {typeof(TImplementation).Name}");

                return instance == null
                    ? throw new InvalidOperationException($"Failed to create instance of {typeof(TImplementation).Name}")
                    : (TInterface)(TImplementation)instance;
            });
        }

        private static IOptions<Service> CreateServiceOptions(IServiceCollection services, string configName, string? endPointOverride)
        {
            var serviceProvider = services.BuildServiceProvider();
            var servicesConfig = serviceProvider.GetRequiredService<IOptions<ServicesConfiguration>>().Value;

            var serviceConfig = (Service?)servicesConfig.GetType().GetProperty(configName)?.GetValue(servicesConfig);

            ValidateServiceConfiguration(serviceConfig, configName);

            var endPointName = endPointOverride ?? serviceConfig?.EndPointName;

            return Options.Create(new Service
            {
                Url = serviceConfig?.Url,
                EndPointName = endPointName,
                BearerToken = serviceConfig?.BearerToken,
                HttpClientName = serviceConfig?.HttpClientName,
                Retries = serviceConfig?.Retries
            });
        }

        private static void ValidateServiceConfiguration(Service? serviceConfig, string configName)
        {
            if (serviceConfig?.Url == null)
            {
                throw new InvalidOperationException($"{configName} Url configuration is missing.");
            }

            if (serviceConfig.EndPointName == null)
            {
                throw new InvalidOperationException($"{configName} EndPointName configuration is missing.");
            }
        }
    }
}
