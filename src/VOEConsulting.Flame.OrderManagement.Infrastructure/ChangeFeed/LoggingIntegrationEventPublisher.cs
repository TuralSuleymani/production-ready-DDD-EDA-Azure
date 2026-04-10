using Microsoft.Extensions.Logging;

namespace VOEConsulting.Flame.OrderManagement.Infrastructure.ChangeFeed;

public sealed class LoggingIntegrationEventPublisher : IIntegrationEventPublisher
{
    private readonly ILogger<LoggingIntegrationEventPublisher> _logger;

    public LoggingIntegrationEventPublisher(ILogger<LoggingIntegrationEventPublisher> logger)
    {
        _logger = logger;
    }

    public Task PublishOutboxAsync(
        string tenantId,
        string outboxItemId,
        string orderId,
        string eventType,
        string payloadJson,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Integration event (outbox {OutboxId}, tenant {TenantId}, order {OrderId}, type {EventType}): {Payload}",
            outboxItemId,
            tenantId,
            orderId,
            eventType,
            payloadJson);
        return Task.CompletedTask;
    }
}
