namespace VOEConsulting.Flame.OrderManagement.Application.Integration;

public sealed record OrderPlacedIntegrationEvent(
    Guid AggregateId,
    string OrderId,
    string CustomerId,
    decimal SubtotalAmount,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal TotalAmount,
    string Currency,
    DateTimeOffset OccurredOnUtc);

public sealed record OrderCancelledIntegrationEvent(
    Guid AggregateId,
    string OrderId,
    DateTimeOffset OccurredOnUtc);

public sealed record OrderShippedIntegrationEvent(
    Guid AggregateId,
    string OrderId,
    DateTimeOffset OccurredOnUtc);
