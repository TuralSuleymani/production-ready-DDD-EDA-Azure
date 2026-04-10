namespace VOEConsulting.Flame.OrderManagement.Application.Outbox;

/// <summary>Integration event envelope stored next to the aggregate (transactional outbox pattern).</summary>
public sealed class OutboxRecord
{
    public required string Id { get; init; }
    public required string TenantId { get; init; }
    public required string OrderId { get; init; }
    public required string EventType { get; init; }
    public required string PayloadJson { get; init; }
    public IReadOnlyDictionary<string, string>? Metadata { get; init; }
    public bool Processed { get; init; }
}
