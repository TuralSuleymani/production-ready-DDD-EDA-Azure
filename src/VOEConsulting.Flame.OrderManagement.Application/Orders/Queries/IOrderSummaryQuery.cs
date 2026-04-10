namespace VOEConsulting.Flame.OrderManagement.Application.Orders.Queries;

/// <summary>Read-side queries against the projected order-summaries container.</summary>
public interface IOrderSummaryQuery
{
    Task<OrderSummaryDto?> GetAsync(string customerId, string orderId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrderSummaryDto>> ListByCustomerAsync(
        string customerId,
        CancellationToken cancellationToken = default);
}
