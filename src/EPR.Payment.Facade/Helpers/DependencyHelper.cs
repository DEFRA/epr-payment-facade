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
            services.AddTransient<TokenAuthorizationHandler>(sp =>
            {
                var servicesConfig = sp.GetRequiredService<IOptions<ServicesConfiguration>>().Value;
                var serviceConfig = servicesConfig.ProducerFeesService;

                if (serviceConfig == null || string.IsNullOrEmpty(serviceConfig.ServiceClientId))
                {
                    throw new InvalidOperationException("ServiceClientId for ProducerFeesService is null or empty.");
                }

                return new TokenAuthorizationHandler(Options.Create(serviceConfig));
            });

            // Explicitly register each service with its configuration

            services.AddTransient<IHttpPaymentServiceHealthCheckService>(sp =>
            {
                var config = sp.GetRequiredService<IOptions<ServicesConfiguration>>().Value.PaymentService;
                ValidateServiceConfiguration(config, "PaymentService");
                return new HttpOnlinePaymentServiceHealthCheckService(
                    sp.GetRequiredService<HttpClient>(),
                    sp.GetRequiredService<IHttpContextAccessor>(),
                    Options.Create(config));
            });

            services.AddTransient<IHttpOnlinePaymentsService>(sp =>
            {
                var config = sp.GetRequiredService<IOptions<ServicesConfiguration>>().Value.PaymentService;
                ValidateServiceConfiguration(config, "PaymentService");
                return new HttpOnlinePaymentsService(
                    sp.GetRequiredService<HttpClient>(),
                    sp.GetRequiredService<IHttpContextAccessor>(),
                    Options.Create(config));
            });

            services.AddTransient<IHttpOfflinePaymentsService>(sp =>
            {
                var config = sp.GetRequiredService<IOptions<ServicesConfiguration>>().Value.OfflinePaymentService;
                ValidateServiceConfiguration(config, "OfflinePaymentService");
                return new HttpOfflinePaymentsService(
                    sp.GetRequiredService<HttpClient>(),
                    sp.GetRequiredService<IHttpContextAccessor>(),
                    Options.Create(config));
            });

            services.AddTransient<IHttpGovPayService>(sp =>
            {
                var config = sp.GetRequiredService<IOptions<ServicesConfiguration>>().Value.GovPayService;
                ValidateServiceConfiguration(config, "GovPayService");
                return new HttpGovPayService(
                    sp.GetRequiredService<HttpClient>(),
                    sp.GetRequiredService<IHttpContextAccessor>(),
                    Options.Create(config));
            });

            services.AddTransient<IHttpProducerFeesService>(sp =>
            {
                var config = sp.GetRequiredService<IOptions<ServicesConfiguration>>().Value.ProducerFeesService;
                ValidateServiceConfiguration(config, "ProducerFeesService");
                return new HttpProducerFeesService(
                    sp.GetRequiredService<HttpClient>(),
                    sp.GetRequiredService<IHttpContextAccessor>(),
                    Options.Create(config));
            });

            services.AddTransient<IHttpComplianceSchemeFeesService>(sp =>
            {
                var config = sp.GetRequiredService<IOptions<ServicesConfiguration>>().Value.ComplianceSchemeFeesService;
                ValidateServiceConfiguration(config, "ComplianceSchemeFeesService");
                return new HttpComplianceSchemeFeesService(
                    sp.GetRequiredService<HttpClient>(),
                    sp.GetRequiredService<IHttpContextAccessor>(),
                    Options.Create(config));
            });

            services.AddTransient<IHttpProducerResubmissionFeesService>(sp =>
            {
                var config = sp.GetRequiredService<IOptions<ServicesConfiguration>>().Value.ProducerFeesService;
                ValidateServiceConfiguration(config, "ProducerFeesService");
                return new HttpProducerResubmissionFeesService(
                    sp.GetRequiredService<HttpClient>(),
                    sp.GetRequiredService<IHttpContextAccessor>(),
                    Options.Create(config));
            });

            services.AddTransient<IHttpComplianceSchemeResubmissionFeesService>(sp =>
            {
                var config = sp.GetRequiredService<IOptions<ServicesConfiguration>>().Value.ComplianceSchemeFeesService;
                ValidateServiceConfiguration(config, "ComplianceSchemeFeesService");
                return new HttpComplianceSchemeResubmissionFeesService(
                    sp.GetRequiredService<HttpClient>(),
                    sp.GetRequiredService<IHttpContextAccessor>(),
                    Options.Create(config));
            });

            return services;
        }

        private static void ValidateServiceConfiguration(Service? serviceConfig, string configName)
        {
            if (serviceConfig == null)
            {
                throw new InvalidOperationException($"{configName} configuration is null.");
            }

            if (string.IsNullOrWhiteSpace(serviceConfig.Url))
            {
                throw new InvalidOperationException($"{configName} Url configuration is missing.");
            }

            if (string.IsNullOrWhiteSpace(serviceConfig.EndPointName))
            {
                throw new InvalidOperationException($"{configName} EndPointName configuration is missing.");
            }
        }
    }
}
