using System.Diagnostics.CodeAnalysis;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EPR.Payment.Facade.Messaging;

[ExcludeFromCodeCoverage]
public class ServiceBusTopicSubscription : IServiceBusTopicSubscription
{
    private const string TopicPath = "topic.new";
    private const string SubscriptionName = "subscription.new";
    private readonly ILogger<ServiceBusTopicSubscription> _logger;
    private readonly ServiceBusClient _client;
    private readonly ServiceBusAdministrationClient _adminClient;
    private ServiceBusProcessor? _processor;
 
    public ServiceBusTopicSubscription(ILogger<ServiceBusTopicSubscription> logger, IConfiguration configuration)
    {
        _logger = logger;

        var connectionString = configuration.GetValue<string>("ServiceBus:ConnectionString");
        var adminConnectionString = configuration.GetValue<string>("ServiceBus:AdminConnectionString");

        if (!string.IsNullOrEmpty(connectionString) && !string.IsNullOrEmpty(adminConnectionString))
        {
            _client = new ServiceBusClient(connectionString);
            _adminClient = new ServiceBusAdministrationClient(adminConnectionString);
        }
    }
 
    public async Task PrepareServiceBusSubscription()
    {
        var serviceBusProcessorOptions = new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = 1,
            AutoCompleteMessages = false,
        };

        try
        {
            var topicExistsResult = await _adminClient.TopicExistsAsync(TopicPath);

            if (!topicExistsResult.HasValue)
            {
                throw new InvalidOperationException(
                    "Unable to get a result when trying to query for the existence of a topic");
            }

            if (!topicExistsResult.Value)
            {
                await _adminClient.CreateTopicAsync(TopicPath);
            }

            var subscriptionExistsResult = await _adminClient.SubscriptionExistsAsync(TopicPath, SubscriptionName);

            if (!subscriptionExistsResult.HasValue)
            {
                throw new InvalidOperationException(
                    "Unable to get a result when trying to query for the existence of a subscription");
            }

            if (!subscriptionExistsResult.Value)
            {
                await _adminClient.CreateSubscriptionAsync(TopicPath, SubscriptionName);
            }

            _processor = _client.CreateProcessor(TopicPath, SubscriptionName, serviceBusProcessorOptions);
            _processor.ProcessMessageAsync += ProcessMessagesAsync;
            _processor.ProcessErrorAsync += ProcessErrorAsync;

            await _processor.StartProcessingAsync();
        }
        catch (Exception ex)
        {
            // we wouldn't have this exception catch for the live service. If the service cannot subscribe to the message bus, it shouldn't start
            _logger.LogError(ex, "An exception occurred while starting up the service bus subscription");
        }
 
    }
 
    private async Task ProcessMessagesAsync(ProcessMessageEventArgs args)
    {
        var myPayload = args.Message.Body.ToObjectFromJson<FooMessage>();
        _logger.LogInformation("Received message: {myPayload}", myPayload);
        await args.CompleteMessageAsync(args.Message);
    }
 
    private Task ProcessErrorAsync(ProcessErrorEventArgs arg)
    {
        _logger.LogError(arg.Exception, "Message handler encountered an exception");
        _logger.LogDebug("- ErrorSource: {ErrorSource}", arg.ErrorSource);
        _logger.LogDebug("- Entity Path: {EntityPath}", arg.EntityPath);
        _logger.LogDebug("- FullyQualifiedNamespace: {FullyQualifiedNamespace}", arg.FullyQualifiedNamespace);
 
        return Task.CompletedTask;
    }
 
    public async ValueTask DisposeAsync()
    {
        if (_processor != null)
        {
            await _processor.DisposeAsync();
        }
 
        if (_client != null)
        {
            await _client.DisposeAsync();
        }
    }
 
    public async Task CloseSubscriptionAsync()
    {
        await _processor!.CloseAsync();
    }
}