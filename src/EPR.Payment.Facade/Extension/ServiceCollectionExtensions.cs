using EPR.Payment.Facade.Services.Interfaces;
using EPR.Payment.Facade.Services;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Payment.Facade.Extension
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDependencies(this IServiceCollection services)
        {


            return services;

        }
    }
}
