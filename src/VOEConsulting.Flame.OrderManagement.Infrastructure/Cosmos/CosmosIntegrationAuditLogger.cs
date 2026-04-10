using Microsoft.Azure.Cosmos;

namespace VOEConsulting.Flame.OrderManagement.Infrastructure.Cosmos;

public sealed class CosmosIntegrationAuditLogger : IIntegrationAuditLogger
{
    private readonly Container _container;

    public CosmosIntegrationAuditLogger(Container auditContainer)
    {
        _container = auditContainer;
    }

    public async Task LogPublishAsync(
        string tenantId,
        string orderId,
        string outboxItemId,
        string eventType,
        string payloadJson,
        CancellationToken cancellationToken = default)
    {
        var doc = new IntegrationAuditCosmosDocument
        {
            Id = outboxItemId,
            TenantId = tenantId,
            OrderId = orderId,
            OutboxItemId = outboxItemId,
            EventType = eventType,
            PayloadJson = payloadJson,
            PublishedAtUtc = DateTimeOffset.UtcNow
        };

        await _container
            .UpsertItemAsync(doc, new PartitionKey(tenantId), cancellationToken: cancellationToken);
    }
}
