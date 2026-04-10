namespace VOEConsulting.Flame.OrderManagement.Application.Orders.Queries;

/// <summary>Operations view (container <c>order-ops-index</c>, PK <c>/opsPartitionKey</c>).</summary>
public sealed record OpsOrderDto(
    string OrderId,
    string TenantId,
    string CustomerId,
    string Status,
    string YearMonth,
    string OpsPartitionKey,
    DateTimeOffset PlacedAt,
    DateTimeOffset UpdatedAt);
