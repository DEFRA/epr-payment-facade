using System.Diagnostics.CodeAnalysis;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EPR.Payment.Facade.Messaging;

[ExcludeFromCodeCoverage]
public class ServiceBusTopicSubscription : IServiceBusTopicSubscription
{
    private const string TOPIC_PATH = "topic.new";
    private const string SUBSCRIPTION_NAME = "subscription.new";
    private readonly ILogger<ServiceBusTopicSubscription> _logger;
    private readonly ServiceBusClient _client;
    private readonly ServiceBusAdministrationClient _adminClient;
    private ServiceBusProcessor? _processor = null;
 
    public ServiceBusTopicSubscription(ILogger<ServiceBusTopicSubscription> logger, IConfiguration configuration)
    {
        _logger = logger;

        var connectionString = configuration.GetValue<string>("ServiceBus:ConnectionString");
        var adminConnectionString = configuration.GetValue<string>("ServiceBus:AdminConnectionString");
        _client = new ServiceBusClient(connectionString);
        _adminClient = new ServiceBusAdministrationClient(adminConnectionString);
    }
 
    public async Task PrepareServiceBusSubscription()
    {
        var _serviceBusProcessorOptions = new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = 1,
            AutoCompleteMessages = false,
        };

        var topicExistsResult = await _adminClient.TopicExistsAsync(TOPIC_PATH);
        
        if (!topicExistsResult.HasValue)
        {
            throw new InvalidOperationException(
                "Unable to get a result when trying to query for the existence of a topic");
        }
        
        if (!topicExistsResult.Value)
        {
            await _adminClient.CreateTopicAsync(TOPIC_PATH);
        }
        
        var subscriptionExistsResult = await _adminClient.SubscriptionExistsAsync(TOPIC_PATH, SUBSCRIPTION_NAME);

        if (!subscriptionExistsResult.HasValue)
        {
            throw new InvalidOperationException(
                "Unable to get a result when trying to query for the existence of a subscription");
        }
        if (!subscriptionExistsResult.Value)
        {
            await _adminClient.CreateSubscriptionAsync(TOPIC_PATH, SUBSCRIPTION_NAME);
        }
        
        _processor = _client.CreateProcessor(TOPIC_PATH, SUBSCRIPTION_NAME, _serviceBusProcessorOptions);
        _processor.ProcessMessageAsync += ProcessMessagesAsync;
        _processor.ProcessErrorAsync += ProcessErrorAsync;
        
 
        await _processor.StartProcessingAsync();
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