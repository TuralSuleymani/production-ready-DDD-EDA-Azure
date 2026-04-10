using Microsoft.Azure.Cosmos;

namespace VOEConsulting.Flame.OrderManagement.Infrastructure.Cosmos;

public sealed class CosmosOutboxProcessedMarker : IOutboxProcessedMarker
{
    private readonly Container _container;

    public CosmosOutboxProcessedMarker(Container container)
    {
        _container = container;
    }

    public async Task MarkProcessedAsync(
        string tenantId,
        string outboxItemId,
        CancellationToken cancellationToken = default)
    {
        await _container.PatchItemAsync<OutboxCosmosDocument>(
                outboxItemId,
                new PartitionKey(tenantId),
                new[] { PatchOperation.Set("/processed", true) },
                cancellationToken: cancellationToken)
            ;
    }
}
