using System.Diagnostics.CodeAnalysis;
using EPR.Payment.Facade.Common.Configuration;
using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.HttpHandlers;
using EPR.Payment.Facade.Common.RESTServices.AccreditationFees;
using EPR.Payment.Facade.Common.RESTServices.AccreditationFees.Interfaces;
using EPR.Payment.Facade.Common.RESTServices.Payments;
using EPR.Payment.Facade.Common.RESTServices.Payments.Interfaces;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees.ComplianceScheme;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees.ComplianceScheme.Interfaces;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees.Producer.Interfaces;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees.ReprocessorOrExporter;
using EPR.Payment.Facade.Common.RESTServices.RegistrationFees.ReprocessorOrExporter.Interfaces;
using EPR.Payment.Facade.Common.RESTServices.ResubmissionFees.ComplianceScheme;
using EPR.Payment.Facade.Common.RESTServices.ResubmissionFees.ComplianceScheme.Interfaces;
using EPR.Payment.Facade.Common.RESTServices.ResubmissionFees.Producer;
using EPR.Payment.Facade.Common.RESTServices.ResubmissionFees.Producer.Interfaces;
using Microsoft.Extensions.Options;

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
            services.Configure<Service>("RexExpoRegistrationFeesService", configuration.GetSection("Services:RexExpoRegistrationFeesService"));
            services.Configure<Service>("PaymentService", configuration.GetSection("Services:PaymentService"));
            services.Configure<Service>("OnlineV2PaymentService", configuration.GetSection("Services:OnlineV2PaymentService"));
            services.Configure<Service>("OfflinePaymentService", configuration.GetSection("Services:OfflinePaymentService"));
            services.Configure<Service>("GovPayService", configuration.GetSection("Services:GovPayService"));
            services.Configure<Service>("PaymentServiceHealthCheck", configuration.GetSection("Services:PaymentServiceHealthCheck"));
            services.Configure<Service>("RexExpoAccreditationFeesService", configuration.GetSection("Services:RexExpoAccreditationFeesService"));

            // Register IHttpContextAccessor
            services.AddHttpContextAccessor();

            // Register the authorization handler
            services.AddTransient(sp =>
            {
                var servicesConfig = sp.GetRequiredService<IOptions<ServicesConfiguration>>().Value;
                var serviceConfig = servicesConfig.ProducerFeesService;

                if (serviceConfig == null || string.IsNullOrEmpty(serviceConfig.ServiceClientId))
                {
                    throw new InvalidOperationException("ServiceClientId for ProducerFeesService is null or empty.");
                }

                var logger = sp.GetRequiredService<ILogger<TokenAuthorizationHandler>>(); // Get logger instance
                return new TokenAuthorizationHandler(Options.Create(serviceConfig), logger);
            });


            // Register HttpClients with TokenAuthorizationHandler and pass configuration to constructors

            services.AddHttpClient<IHttpProducerFeesService, HttpProducerFeesService>()
                .AddHttpMessageHandler<TokenAuthorizationHandler>()
                .ConfigureHttpClient((sp, client) =>
                {
                    var config = sp.GetRequiredService<IOptions<ServicesConfiguration>>().Value.ProducerFeesService;
                    ValidateServiceConfiguration(config, ExceptionMessages.ProducerFeesServiceBaseUrlMissing);
                    client.BaseAddress = new Uri(config.Url!);
                });

            services.AddHttpClient<IHttpComplianceSchemeFeesService, HttpComplianceSchemeFeesService>()
                .AddHttpMessageHandler<TokenAuthorizationHandler>()
                .ConfigureHttpClient((sp, client) =>
                {
                    var config = sp.GetRequiredService<IOptions<ServicesConfiguration>>().Value.ComplianceSchemeFeesService;
                    ValidateServiceConfiguration(config, ExceptionMessages.ComplianceSchemeServiceUrlMissing);
                    client.BaseAddress = new Uri(config.Url!);
                });

            services.AddHttpClient<IHttpProducerResubmissionFeesService, HttpProducerResubmissionFeesService>()
                .AddHttpMessageHandler<TokenAuthorizationHandler>()
                .ConfigureHttpClient((sp, client) =>
                {
                    var config = sp.GetRequiredService<IOptions<ServicesConfiguration>>().Value.ProducerResubmissionFeesService;
                    ValidateServiceConfiguration(config, ExceptionMessages.ProducerResubmissionFeesServiceBaseUrlMissing);
                    client.BaseAddress = new Uri(config.Url!);
                });

            services.AddHttpClient<IHttpComplianceSchemeResubmissionFeesService, HttpComplianceSchemeResubmissionFeesService>()
                .AddHttpMessageHandler<TokenAuthorizationHandler>()
                .ConfigureHttpClient((sp, client) =>
                {
                    var config = sp.GetRequiredService<IOptions<ServicesConfiguration>>().Value.ComplianceSchemeFeesService;
                    ValidateServiceConfiguration(config, ExceptionMessages.ComplianceSchemeServiceUrlMissing);
                    client.BaseAddress = new Uri(config.Url!);
                });
            
            services.AddHttpClient<IHttpReprocessorExporterRegistrationFeesService, HttpReprocessorExporterRegistrationFeesService>()
                .AddHttpMessageHandler<TokenAuthorizationHandler>()
                .ConfigureHttpClient((sp, client) =>
                {
                    var config = sp.GetRequiredService<IOptions<ServicesConfiguration>>().Value.RexExpoRegistrationFeesService;
                    ValidateServiceConfiguration(config, ExceptionMessages.RegistrationFeesServiceBaseUrlMissing);
                    client.BaseAddress = new Uri(config.Url!);
                });

            services.AddHttpClient<IHttpOnlinePaymentsService, HttpOnlinePaymentsService>()
                .AddHttpMessageHandler<TokenAuthorizationHandler>()
                .ConfigureHttpClient((sp, client) =>
                {
                    var config = sp.GetRequiredService<IOptions<ServicesConfiguration>>().Value.PaymentService;
                    ValidateServiceConfiguration(config, ExceptionMessages.OnlinePaymentServiceBaseUrlMissing);
                    client.BaseAddress = new Uri(config.Url!);
                });

            services.AddHttpClient<IHttpOnlinePaymentsV2Service, HttpOnlinePaymentsV2Service>()
               .AddHttpMessageHandler<TokenAuthorizationHandler>()
               .ConfigureHttpClient((sp, client) =>
               {
                   var config = sp.GetRequiredService<IOptions<ServicesConfiguration>>().Value.OnlineV2PaymentService;
                   ValidateServiceConfiguration(config, ExceptionMessages.OnlinePaymentServiceBaseUrlMissing);
                   client.BaseAddress = new Uri(config.Url!);
               });            

            services.AddHttpClient<IHttpOfflinePaymentsService, HttpOfflinePaymentsService>()
                .AddHttpMessageHandler<TokenAuthorizationHandler>()
                .ConfigureHttpClient((sp, client) =>
                {
                    var config = sp.GetRequiredService<IOptions<ServicesConfiguration>>().Value.OfflinePaymentService;
                    ValidateServiceConfiguration(config, ExceptionMessages.OfflinePaymentServiceBaseUrlMissing);
                    client.BaseAddress = new Uri(config.Url!);
                });


            services.AddHttpClient<IHttpAccreditationFeesCalculatorService, HttpAccreditationFeesCalculatorService>()
               .AddHttpMessageHandler<TokenAuthorizationHandler>()
               .ConfigureHttpClient((sp, client) =>
               {
                   var config = sp.GetRequiredService<IOptions<ServicesConfiguration>>().Value.RexExpoAccreditationFeesService;
                   ValidateServiceConfiguration(config, ExceptionMessages.ReprocessorOrExporterAccreditationFeesServiceBaseUrlMissing);
                   client.BaseAddress = new Uri(config.Url!);
               });

            // Register additional services without TokenAuthorizationHandler
            services.AddTransient<IHttpGovPayService>(sp =>
            {
                var config = sp.GetRequiredService<IOptions<ServicesConfiguration>>().Value.GovPayService;
                ValidateServiceConfiguration(config, ExceptionMessages.GovPayServiceBaseUrlMissing);

                var httpClient = sp.GetRequiredService<HttpClient>();

                if (!string.IsNullOrWhiteSpace(config.BearerToken))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", config.BearerToken);
                }
                else
                {
                    throw new InvalidOperationException(ExceptionMessages.GovPayServiceBearerTokenMissing);
                }

                return new HttpGovPayService(httpClient, sp.GetRequiredService<IHttpContextAccessor>(), Options.Create(config));
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