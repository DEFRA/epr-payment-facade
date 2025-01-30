using EPR.Payment.Facade.HealthCheck;
using EPR.Payment.Facade.Services.Payments;
using EPR.Payment.Facade.Services.Payments.Interfaces;
using EPR.Payment.Facade.Services.RegistrationFees.ComplianceScheme;
using EPR.Payment.Facade.Services.RegistrationFees.ComplianceScheme.Interfaces;
using EPR.Payment.Facade.Services.RegistrationFees.Producer;
using EPR.Payment.Facade.Services.RegistrationFees.Producer.Interfaces;
using EPR.Payment.Facade.Services.ResubmissionFees.ComplianceScheme;
using EPR.Payment.Facade.Services.ResubmissionFees.ComplianceScheme.Interfaces;
using EPR.Payment.Facade.Services.ResubmissionFees.Producer;
using EPR.Payment.Facade.Services.ResubmissionFees.Producer.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics.CodeAnalysis;

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

            // Register payments services
            services.AddScoped<IOnlinePaymentsService, OnlinePaymentsService>();
            services.AddScoped<IOfflinePaymentsService, OfflinePaymentsService>();

            services.AddScoped<IProducerFeesService, ProducerFeesService>();
            services.AddScoped<IProducerResubmissionFeesService, ProducerResubmissionFeesService>();
            services.AddScoped<IComplianceSchemeCalculatorService, ComplianceSchemeCalculatorService>();
            services.AddScoped<IComplianceSchemeResubmissionFeesService, ComplianceSchemeResubmissionFeesService>();

            return services;
        }
    }
}