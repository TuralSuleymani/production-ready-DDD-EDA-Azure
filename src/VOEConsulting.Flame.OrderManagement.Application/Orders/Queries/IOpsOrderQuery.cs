namespace VOEConsulting.Flame.OrderManagement.Application.Orders.Queries;

/// <summary>Ops list: single-partition query using <c>tenantId|status|yyyy-MM</c> partition key.</summary>
public interface IOpsOrderQuery
{
    /// <param name="yearMonth">UTC month bucket <c>yyyy-MM</c> (matches projector).</param>
    Task<IReadOnlyList<OpsOrderDto>> ListByStatusMonthAsync(
        string tenantId,
        string status,
        string yearMonth,
        CancellationToken cancellationToken = default);
}
