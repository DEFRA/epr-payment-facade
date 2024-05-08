using EPR.Payment.Facade.Services.Interfaces;
using EPR.Payment.Facade.Services;
using System.Diagnostics.CodeAnalysis;
using EPR.Payment.Facade.Common.RESTServices.Interfaces;
using EPR.Payment.Facade.Common.RESTServices;

namespace EPR.Payment.Facade.Extension
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDependencies(this IServiceCollection services)
        {
            services.AddScoped<IFeesService, FeesService>();
            services.AddScoped<IPaymentsService, PaymentsService>();
            services.AddScoped<IHttpFeesService, HttpFeesService>();
            services.AddScoped<IHttpGovPayService, HttpGovPayService>();
            services.AddScoped<IHttpPaymentsService, HttpPaymentsService>();
            services.AddHttpContextAccessor();
            services.AddHttpClient();

            return services;

        }
    }
}
