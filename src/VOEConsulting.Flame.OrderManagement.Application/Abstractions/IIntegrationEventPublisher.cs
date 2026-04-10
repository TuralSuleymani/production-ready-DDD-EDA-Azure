namespace VOEConsulting.Flame.OrderManagement.Application.Abstractions;

public interface IIntegrationEventPublisher
{
    Task PublishOutboxAsync(
        string tenantId,
        string outboxItemId,
        string orderId,
        string eventType,
        string payloadJson,
        CancellationToken cancellationToken = default);
}
