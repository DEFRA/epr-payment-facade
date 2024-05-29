using System.Diagnostics.CodeAnalysis;
using EPR.Payment.Facade.Common.RESTServices.Interfaces;
using EPR.Payment.Facade.Common.RESTServices;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using EPR.Payment.Facade.HealthCheck;
using EPR.Payment.Facade.Services.Fees.Registration;
using EPR.Payment.Facade.Services.Fees.Registration.Interfaces;
using EPR.Payment.Facade.Services.Payments.Interfaces;
using EPR.Payment.Facade.Services.Payments;

namespace EPR.Payment.Facade.Extension
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtensions
    {
        private static readonly string Ready = "ready";
        public static IServiceCollection AddDependencies(this IServiceCollection services)
        {
            //services.AddScoped<IFeesService, FeesService>();

            services.AddControllers();
            services.AddScoped<IFeeCalculatorService<RegistrationFeeRequest>, RegistrationFeeCalculatorService>();
            //services.AddScoped<IFeeCalculatorService<AccreditationFeeRequest>, AccreditationFeeCalculatorService>();
            services.AddScoped<IFeeCalculatorFactory, FeeCalculatorFactory>();
            //services.AddScoped<IHttpFeesService, HttpFeesService>();

            services.AddScoped<IPaymentsService, PaymentsService>();            
            services.AddScoped<IHttpGovPayService, HttpGovPayService>();
            services.AddScoped<IHttpPaymentsService, HttpPaymentsService>();
            services.AddHttpContextAccessor();
            services.AddHttpClient();

            return services;

        }

        public static IServiceCollection AddServiceHealthChecks(this IServiceCollection services) 
        {
            services.AddHealthChecks()
                .AddCheck<PaymentsFacadeHealthCheck>(PaymentsFacadeHealthCheck.HealthCheckResultDescription,
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new[] { Ready });
            return services;
        }
    }
}
