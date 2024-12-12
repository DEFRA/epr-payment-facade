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

            // Register individual service configurations
            services.Configure<Service>("ProducerFeesService", configuration.GetSection("Services:ProducerFeesService"));
            services.Configure<Service>("ComplianceSchemeFeesService", configuration.GetSection("Services:ComplianceSchemeFeesService"));
            services.Configure<Service>("ProducerResubmissionFeesService", configuration.GetSection("Services:ProducerResubmissionFeesService"));

            // Register IHttpContextAccessor
            services.AddHttpContextAccessor();

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

            // Register HttpClients with TokenAuthorizationHandler and pass configuration to constructors

            services.AddHttpClient<IHttpProducerFeesService, HttpProducerFeesService>()
                    .AddHttpMessageHandler<TokenAuthorizationHandler>()
                    .ConfigureHttpClient((sp, client) =>
                    {
                        var config = sp.GetRequiredService<IOptions<ServicesConfiguration>>().Value.ProducerFeesService;
                        ValidateServiceConfiguration(config, "ProducerFeesService");
                        client.BaseAddress = new Uri(config.Url);
                    });

            services.AddHttpClient<IHttpComplianceSchemeFeesService, HttpComplianceSchemeFeesService>()
                    .AddHttpMessageHandler<TokenAuthorizationHandler>()
                    .ConfigureHttpClient((sp, client) =>
                    {
                        var config = sp.GetRequiredService<IOptions<ServicesConfiguration>>().Value.ComplianceSchemeFeesService;
                        ValidateServiceConfiguration(config, "ComplianceSchemeFeesService");
                        client.BaseAddress = new Uri(config.Url);
                    });

            services.AddHttpClient<IHttpProducerResubmissionFeesService, HttpProducerResubmissionFeesService>()
                    .AddHttpMessageHandler<TokenAuthorizationHandler>()
                    .ConfigureHttpClient((sp, client) =>
                    {
                        var config = sp.GetRequiredService<IOptions<ServicesConfiguration>>().Value.ProducerFeesService;
                        ValidateServiceConfiguration(config, "ProducerFeesService");
                        client.BaseAddress = new Uri(config.Url);
                    });

            services.AddHttpClient<IHttpComplianceSchemeResubmissionFeesService, HttpComplianceSchemeResubmissionFeesService>()
                    .AddHttpMessageHandler<TokenAuthorizationHandler>()
                    .ConfigureHttpClient((sp, client) =>
                    {
                        var config = sp.GetRequiredService<IOptions<ServicesConfiguration>>().Value.ComplianceSchemeFeesService;
                        ValidateServiceConfiguration(config, "ComplianceSchemeFeesService");
                        client.BaseAddress = new Uri(config.Url);
                    });

            services.AddHttpClient<IHttpPaymentServiceHealthCheckService, HttpOnlinePaymentServiceHealthCheckService>()
                    .AddHttpMessageHandler<TokenAuthorizationHandler>()
                    .ConfigureHttpClient((sp, client) =>
                    {
                        var config = sp.GetRequiredService<IOptions<ServicesConfiguration>>().Value.PaymentService;
                        ValidateServiceConfiguration(config, "PaymentService");
                        client.BaseAddress = new Uri(config.Url);
                    });

            services.AddHttpClient<IHttpOnlinePaymentsService, HttpOnlinePaymentsService>()
                    .AddHttpMessageHandler<TokenAuthorizationHandler>()
                    .ConfigureHttpClient((sp, client) =>
                    {
                        var config = sp.GetRequiredService<IOptions<ServicesConfiguration>>().Value.PaymentService;
                        ValidateServiceConfiguration(config, "PaymentService");
                        client.BaseAddress = new Uri(config.Url);
                    });

            services.AddHttpClient<IHttpOfflinePaymentsService, HttpOfflinePaymentsService>()
                    .AddHttpMessageHandler<TokenAuthorizationHandler>()
                    .ConfigureHttpClient((sp, client) =>
                    {
                        var config = sp.GetRequiredService<IOptions<ServicesConfiguration>>().Value.OfflinePaymentService;
                        ValidateServiceConfiguration(config, "OfflinePaymentService");
                        client.BaseAddress = new Uri(config.Url);
                    });

            // Register additional services without TokenAuthorizationHandler
            services.AddTransient<IHttpGovPayService>(sp =>
            {
                var config = sp.GetRequiredService<IOptions<ServicesConfiguration>>().Value.GovPayService;
                ValidateServiceConfiguration(config, "GovPayService");
                return new HttpGovPayService(
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
