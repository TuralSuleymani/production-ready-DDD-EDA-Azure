namespace VOEConsulting.Flame.OrderManagement.Infrastructure.Integration;

public sealed class NullIntegrationAuditLogger : IIntegrationAuditLogger
{
    public Task LogPublishAsync(
        string tenantId,
        string orderId,
        string outboxItemId,
        string eventType,
        string payloadJson,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
