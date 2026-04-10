using VOEConsulting.Flame.OrderManagement.Domain.Orders.Events;

namespace VOEConsulting.Flame.OrderManagement.Application.Integration;

public static class OrderIntegrationEventMapper
{
    public static (string EventType, object Payload) Map(IDomainEvent domainEvent)
    {
        return domainEvent switch
        {
            OrderPlacedEvent e => (
                nameof(OrderPlacedIntegrationEvent),
                new OrderPlacedIntegrationEvent(
                    e.AggregateId,
                    e.OrderId,
                    e.CustomerId,
                    e.SubtotalAmount,
                    e.DiscountAmount,
                    e.TaxAmount,
                    e.TotalAmount,
                    e.Currency,
                    e.OccurredOnUtc)),
            OrderCancelledEvent e => (
                nameof(OrderCancelledIntegrationEvent),
                new OrderCancelledIntegrationEvent(e.AggregateId, e.OrderId, e.OccurredOnUtc)),
            OrderShippedEvent e => (
                nameof(OrderShippedIntegrationEvent),
                new OrderShippedIntegrationEvent(e.AggregateId, e.OrderId, e.OccurredOnUtc)),
            _ => throw new NotSupportedException($"No integration mapping for {domainEvent.GetType().Name}.")
        };
    }
}
