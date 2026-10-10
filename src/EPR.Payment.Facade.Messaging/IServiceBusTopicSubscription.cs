namespace EPR.Payment.Facade.Messaging;

public interface IServiceBusTopicSubscription
{
    Task PrepareServiceBusSubscription();
    Task CloseSubscriptionAsync();
    ValueTask DisposeAsync();
}