using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EPR.Payment.Facade.Messaging;

[ExcludeFromCodeCoverage]
public class ServiceBusTopicSender : IServiceBusTopicPublisher
{
    private const string TopicPath = "topic.new";
    private readonly ILogger<ServiceBusTopicSender> _logger;
    private readonly ServiceBusClient _client;
    private Azure.Messaging.ServiceBus.ServiceBusSender _clientSender = null!;
    private readonly string? _adminConnectionString;

    public ServiceBusTopicSender(ILogger<ServiceBusTopicSender> logger, IConfiguration configuration)
    {
        _logger = logger;
 
        var connectionString = configuration.GetValue<string>("ServiceBus:ConnectionString");
        _adminConnectionString = configuration.GetValue<string>("ServiceBus:AdminConnectionString");
        _client = new ServiceBusClient(connectionString);
    }
 
    public async Task SendMessage(FooMessage payload)
    {
        await SetupClient();
        string messagePayload = JsonSerializer.Serialize(payload);
        var message = new ServiceBusMessage(messagePayload);
 
        try
        {
            _logger.LogInformation("Sending message: {messagePayload}", messagePayload);
            await _clientSender.SendMessageAsync(message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to send message: {messagePayload}", messagePayload);
        }
    }

    private async Task SetupClient()
    {
        if (_clientSender == null)
        {
            var adminClient = new ServiceBusAdministrationClient(_adminConnectionString);
            
            var topicExistsResult = await adminClient.TopicExistsAsync(TopicPath);

            if (!topicExistsResult.HasValue)
            {
                throw new InvalidOperationException(
                    "Unable to get a result when trying to query for the existence of a topic");
            }
            
            if (!topicExistsResult.Value)
            {
                await adminClient.CreateTopicAsync(TopicPath);
            }

            _clientSender = _client.CreateSender(TopicPath);
        }
    }
}