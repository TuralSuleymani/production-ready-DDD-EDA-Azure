namespace VOEConsulting.Flame.OrderManagement.Application.Abstractions;

public interface IIntegrationAuditLogger
{
    Task LogPublishAsync(
        string tenantId,
        string orderId,
        string outboxItemId,
        string eventType,
        string payloadJson,
        CancellationToken cancellationToken = default);
}
