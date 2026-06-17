using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace EPR.Payment.Facade.Messaging;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMessaging(this IServiceCollection services)
    {
        services.AddSingleton<IServiceBusTopicPublisher, ServiceBusTopicSender>();
        services.AddSingleton<IServiceBusTopicSubscription, ServiceBusTopicSubscription>();
 
        services.AddHostedService<WorkerServiceBus>();
        
        return services;
    }
}