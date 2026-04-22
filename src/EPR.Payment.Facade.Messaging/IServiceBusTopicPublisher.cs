namespace EPR.Payment.Facade.Messaging;

public interface IServiceBusTopicPublisher
{
    Task SendMessage(FooMessage payload);
}