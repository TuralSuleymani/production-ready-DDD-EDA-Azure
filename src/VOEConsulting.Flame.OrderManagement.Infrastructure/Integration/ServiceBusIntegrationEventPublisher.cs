using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;

namespace VOEConsulting.Flame.OrderManagement.Infrastructure.Integration;

public sealed class ServiceBusIntegrationEventPublisher : IIntegrationEventPublisher
{
    private readonly ServiceBusSender _sender;
    private readonly ILogger<ServiceBusIntegrationEventPublisher> _logger;

    public ServiceBusIntegrationEventPublisher(
        ServiceBusSender sender,
        ILogger<ServiceBusIntegrationEventPublisher> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    public async Task PublishOutboxAsync(
        string tenantId,
        string outboxItemId,
        string orderId,
        string eventType,
        string payloadJson,
        CancellationToken cancellationToken = default)
    {
        var message = new ServiceBusMessage(BinaryData.FromString(string.IsNullOrEmpty(payloadJson) ? "{}" : payloadJson))
        {
            ContentType = "application/json",
            Subject = eventType,
            MessageId = outboxItemId,
            CorrelationId = orderId
        };
        message.ApplicationProperties["tenantId"] = tenantId;
        message.ApplicationProperties["orderId"] = orderId;
        message.ApplicationProperties["outboxItemId"] = outboxItemId;
        message.ApplicationProperties["eventType"] = eventType;

        await _sender.SendMessageAsync(message, cancellationToken);
        _logger.LogDebug("Published outbox {OutboxId} type {EventType} for tenant {TenantId}.", outboxItemId, eventType, tenantId);
    }
}
