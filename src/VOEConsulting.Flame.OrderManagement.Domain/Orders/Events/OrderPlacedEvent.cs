namespace VOEConsulting.Flame.OrderManagement.Domain.Orders.Events;

public sealed class OrderPlacedEvent : DomainEvent
{
    public OrderPlacedEvent(
        Guid aggregateId,
        DateTimeOffset occurredOnUtc,
        string orderId,
        string customerId,
        decimal subtotalAmount,
        decimal discountAmount,
        decimal taxAmount,
        decimal totalAmount,
        string currency)
        : base(aggregateId, occurredOnUtc, OrderConstants.AggregateTypeName)
    {
        OrderId = orderId;
        CustomerId = customerId;
        SubtotalAmount = subtotalAmount;
        DiscountAmount = discountAmount;
        TaxAmount = taxAmount;
        TotalAmount = totalAmount;
        Currency = currency;
    }

    public string OrderId { get; }
    public string CustomerId { get; }
    public decimal SubtotalAmount { get; }
    public decimal DiscountAmount { get; }
    public decimal TaxAmount { get; }
    public decimal TotalAmount { get; }
    public string Currency { get; }
}
