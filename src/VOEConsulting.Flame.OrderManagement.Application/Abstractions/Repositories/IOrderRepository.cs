using CSharpFunctionalExtensions;
using VOEConsulting.Flame.OrderManagement.Domain.Orders;

namespace VOEConsulting.Flame.OrderManagement.Application.Abstractions.Repositories;

public interface IOrderRepository
{
    Task<OrderPersistenceState?> GetAsync(
        string tenantId,
        string orderId,
        CancellationToken cancellationToken = default);

    Task<UnitResult<DomainError>> SaveAsync(
        string tenantId,
        Order order,
        string? expectedETag,
        IReadOnlyList<OutboxRecord> newOutboxItems,
        CancellationToken cancellationToken = default);
}
