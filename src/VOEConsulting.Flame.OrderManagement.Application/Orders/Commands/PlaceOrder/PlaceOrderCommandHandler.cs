using CSharpFunctionalExtensions;
using VOEConsulting.Flame.Domain.Common.ValueObjects;
using VOEConsulting.Flame.OrderManagement.Domain.Orders;

namespace VOEConsulting.Flame.OrderManagement.Application.Orders.Commands.PlaceOrder;

public sealed class PlaceOrderCommandHandler : OrderCommandHandlerBase<PlaceOrderCommand>
{
    public PlaceOrderCommandHandler(IOrderRepository orders)
        : base(orders)
    {
    }

    protected override Task<Result<SaveContext, DomainError>> ExecuteAsync(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        var currencyResult = Currency.Create(request.Currency);
        if (currencyResult.IsFailure)
            return Task.FromResult(Result.Failure<SaveContext, DomainError>(currencyResult.Error));

        var currency = currencyResult.Value;

        var total = Money.Create(request.TotalAmount, currency);
        if (total.IsFailure)
            return Task.FromResult(Result.Failure<SaveContext, DomainError>(total.Error));

        var subtotalAmount = request.SubtotalAmount ?? request.TotalAmount;
        var discountAmount = request.DiscountAmount ?? 0m;
        var taxAmount = request.TaxAmount ?? 0m;

        var subtotal = Money.Create(subtotalAmount, currency);
        if (subtotal.IsFailure)
            return Task.FromResult(Result.Failure<SaveContext, DomainError>(subtotal.Error));

        var discount = Money.Create(discountAmount, currency);
        if (discount.IsFailure)
            return Task.FromResult(Result.Failure<SaveContext, DomainError>(discount.Error));

        var tax = Money.Create(taxAmount, currency);
        if (tax.IsFailure)
            return Task.FromResult(Result.Failure<SaveContext, DomainError>(tax.Error));

        var breakdown = MoneyBreakdown.Create(subtotal.Value, discount.Value, tax.Value);
        if (breakdown.IsFailure)
            return Task.FromResult(Result.Failure<SaveContext, DomainError>(breakdown.Error));

        if (breakdown.Value.Total.Amount != total.Value.Amount)
            return Task.FromResult(Result.Failure<SaveContext, DomainError>(
                DomainError.BadRequest("TotalAmount must equal SubtotalAmount - DiscountAmount + TaxAmount.")));

        var order = Order.PlaceNew(request.OrderId, request.CustomerId, breakdown.Value).Value;
        return Task.FromResult(Result.Success<SaveContext, DomainError>(new SaveContext(request.TenantId, request.OrderId, order, ExpectedETag: null)));
    }
}

