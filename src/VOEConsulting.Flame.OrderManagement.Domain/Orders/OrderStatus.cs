using Ardalis.SmartEnum;

namespace VOEConsulting.Flame.OrderManagement.Domain.Orders;

public sealed class OrderStatus : SmartEnum<OrderStatus>
{
    public static readonly OrderStatus Placed = new(nameof(Placed), 0);
    public static readonly OrderStatus Shipped = new(nameof(Shipped), 1);
    public static readonly OrderStatus Cancelled = new(nameof(Cancelled), 2);

    private OrderStatus(string name, int value)
        : base(name, value)
    {
    }
}
