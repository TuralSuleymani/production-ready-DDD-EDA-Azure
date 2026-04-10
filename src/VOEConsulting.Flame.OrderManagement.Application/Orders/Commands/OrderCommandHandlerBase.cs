using CSharpFunctionalExtensions;
using VOEConsulting.Flame.OrderManagement.Application.Abstractions;
using VOEConsulting.Flame.OrderManagement.Domain.Orders;

namespace VOEConsulting.Flame.OrderManagement.Application.Orders.Commands;

public abstract class OrderCommandHandlerBase<TCommand> : ICommandHandler<TCommand, UnitResult<DomainError>>
    where TCommand : ICommand<UnitResult<DomainError>>
{
    private readonly IOrderRepository _orders;

    protected OrderCommandHandlerBase(IOrderRepository orders)
    {
        _orders = orders;
    }

    public async Task<UnitResult<DomainError>> Handle(TCommand request, CancellationToken cancellationToken)
    {
        var execute = await ExecuteAsync(request, cancellationToken);
        if (execute.IsFailure)
            return UnitResult.Failure(execute.Error);

        var ctx = execute.Value;
        var outbox = ctx.Order.DomainEvents.ToOutboxRecords(ctx.TenantId, ctx.OrderId);
        if (outbox.Count == 0)
            return UnitResult.Success<DomainError>();

        return await _orders
            .SaveAsync(ctx.TenantId, ctx.Order, ctx.ExpectedETag, outbox, cancellationToken);
    }

    protected abstract Task<Result<SaveContext, DomainError>> ExecuteAsync(
        TCommand request,
        CancellationToken cancellationToken);

    protected async Task<Result<OrderPersistenceState, DomainError>> GetOrderOrNotFoundAsync(
        string tenantId,
        string orderId,
        CancellationToken cancellationToken)
    {
        var state = await _orders.GetAsync(tenantId, orderId, cancellationToken);
        if (state is null)
            return Result.Failure<OrderPersistenceState, DomainError>(DomainError.NotFound($"Order '{orderId}' was not found."));

        return Result.Success<OrderPersistenceState, DomainError>(state);
    }

    protected sealed record SaveContext(string TenantId, string OrderId, Order Order, string? ExpectedETag);
}

