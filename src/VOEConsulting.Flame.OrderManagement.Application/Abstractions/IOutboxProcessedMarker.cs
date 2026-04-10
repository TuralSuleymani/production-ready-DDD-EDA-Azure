namespace VOEConsulting.Flame.OrderManagement.Application.Abstractions;

public interface IOutboxProcessedMarker
{
    Task MarkProcessedAsync(string tenantId, string outboxItemId, CancellationToken cancellationToken = default);
}
