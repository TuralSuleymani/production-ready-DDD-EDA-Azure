using CSharpFunctionalExtensions;
using VOEConsulting.Flame.OrderManagement.Application.Orders.Commands;

namespace VOEConsulting.Flame.OrderManagement.Application.Orders.Commands.MarkShipped;

public sealed class MarkShippedCommandHandler : OrderCommandHandlerBase<MarkShippedCommand>
{
    public MarkShippedCommandHandler(IOrderRepository orders)
        : base(orders)
    {
    }

    protected override async Task<Result<SaveContext, DomainError>> ExecuteAsync(MarkShippedCommand request, CancellationToken cancellationToken)
    {
        var stateResult = await GetOrderOrNotFoundAsync(request.TenantId, request.OrderId, cancellationToken);
        if (stateResult.IsFailure)
            return Result.Failure<SaveContext, DomainError>(stateResult.Error);

        var state = stateResult.Value;
        var order = state.Order;
        var ship = order.MarkShipped();
        if (ship.IsFailure)
            return Result.Failure<SaveContext, DomainError>(ship.Error);

        return Result.Success<SaveContext, DomainError>(new SaveContext(request.TenantId, request.OrderId, order, state.ETag));
    }
}

