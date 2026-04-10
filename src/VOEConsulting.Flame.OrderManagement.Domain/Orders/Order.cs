using CSharpFunctionalExtensions;
using VOEConsulting.Flame.Domain.Common;
using VOEConsulting.Flame.Domain.Common.ValueObjects;

namespace VOEConsulting.Flame.OrderManagement.Domain.Orders;

public sealed class Order : AggregateRoot<Order>
{
    private Order(
        Id<Order> id,
        string orderId,
        string customerId,
        MoneyBreakdown pricing,
        OrderStatus status)
        : base(id)
    {
        OrderId = orderId;
        CustomerId = customerId;
        Pricing = pricing;
        Status = status;
    }

    private Order(
        Id<Order> id,
        string orderId,
        string customerId,
        MoneyBreakdown pricing,
        OrderStatus status,
        DateTimeOffset createdAtUtc,
        DateTimeOffset lastModifiedAtUtc)
        : base(id, createdAtUtc, lastModifiedAtUtc)
    {
        OrderId = orderId;
        CustomerId = customerId;
        Pricing = pricing;
        Status = status;
    }

    public string OrderId { get; private set; }
    public string CustomerId { get; private set; }
    public MoneyBreakdown Pricing { get; private set; }
    public OrderStatus Status { get; private set; }

    public static Result<Order, DomainError> PlaceNew(
        string orderId,
        string customerId,
        MoneyBreakdown pricing)
    {
        var id = Id<Order>.New();
        var order = new Order(id, orderId, customerId, pricing, OrderStatus.Placed);
        order.RaiseDomainEvent(new OrderPlacedEvent(
            id.Value,
            DateTimeOffset.UtcNow,
            orderId,
            customerId,
            pricing.Subtotal.Amount,
            pricing.Discount.Amount,
            pricing.Tax.Amount,
            pricing.Total.Amount,
            pricing.Total.Currency.Code));
        return Result.Success<Order, DomainError>(order);
    }

    internal static Order Rehydrate(
        Id<Order> id,
        string orderId,
        string customerId,
        MoneyBreakdown pricing,
        OrderStatus status,
        DateTimeOffset createdAtUtc,
        DateTimeOffset lastModifiedAtUtc)
    {
        return new Order(id, orderId, customerId, pricing, status, createdAtUtc, lastModifiedAtUtc);
    }

    public UnitResult<DomainError> Cancel()
    {
        if (Status != OrderStatus.Placed)
            return UnitResult.Failure(DomainError.BadRequest("Only placed orders can be cancelled."));

        Status = OrderStatus.Cancelled;
        RaiseDomainEvent(new OrderCancelledEvent(Id.Value, DateTimeOffset.UtcNow, OrderId));
        return UnitResult.Success<DomainError>();
    }

    public UnitResult<DomainError> MarkShipped()
    {
        if (Status != OrderStatus.Placed)
            return UnitResult.Failure(DomainError.BadRequest("Only placed orders can be shipped."));

        Status = OrderStatus.Shipped;
        RaiseDomainEvent(new OrderShippedEvent(Id.Value, DateTimeOffset.UtcNow, OrderId));
        return UnitResult.Success<DomainError>();
    }
}
