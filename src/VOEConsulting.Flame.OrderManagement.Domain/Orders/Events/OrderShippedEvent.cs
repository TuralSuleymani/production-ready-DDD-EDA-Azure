namespace VOEConsulting.Flame.OrderManagement.Domain.Orders.Events;

public sealed class OrderShippedEvent : DomainEvent
{
    public OrderShippedEvent(Guid aggregateId, DateTimeOffset occurredOnUtc, string orderId)
        : base(aggregateId, occurredOnUtc, OrderConstants.AggregateTypeName)
    {
        OrderId = orderId;
    }

    public string OrderId { get; }
}
