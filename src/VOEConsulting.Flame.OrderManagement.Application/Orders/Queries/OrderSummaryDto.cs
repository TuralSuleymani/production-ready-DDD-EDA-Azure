namespace VOEConsulting.Flame.OrderManagement.Application.Orders.Queries;

public sealed record OrderSummaryDto(
    string OrderId,
    string CustomerId,
    string TenantId,
    string Status,
    DateTimeOffset UpdatedAt,
    DateTimeOffset PlacedAt);
