using CSharpFunctionalExtensions;

namespace VOEConsulting.Flame.OrderManagement.Application.Orders.Commands.CancelOrder;

public sealed class CancelOrderCommandHandler : OrderCommandHandlerBase<CancelOrderCommand>
{
    public CancelOrderCommandHandler(IOrderRepository orders)
        : base(orders)
    {
    }

    protected override async Task<Result<SaveContext, DomainError>> ExecuteAsync(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var stateResult = await GetOrderOrNotFoundAsync(request.TenantId, request.OrderId, cancellationToken);
        if (stateResult.IsFailure)
            return Result.Failure<SaveContext, DomainError>(stateResult.Error);

        var state = stateResult.Value;
        var order = state.Order;
        var cancel = order.Cancel();
        if (cancel.IsFailure)
            return Result.Failure<SaveContext, DomainError>(cancel.Error);

        return Result.Success<SaveContext, DomainError>(new SaveContext(request.TenantId, request.OrderId, order, state.ETag));
    }
}

