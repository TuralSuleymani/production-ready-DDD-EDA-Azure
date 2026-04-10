using FluentValidation;

namespace VOEConsulting.Flame.OrderManagement.Application.Orders.Commands.PlaceOrder;

public sealed class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.TotalAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3)
            .Matches("^[A-Za-z]{3}$");

        When(x => x.SubtotalAmount.HasValue, () => RuleFor(x => x.SubtotalAmount!.Value).GreaterThanOrEqualTo(0));
        When(x => x.DiscountAmount.HasValue, () => RuleFor(x => x.DiscountAmount!.Value).GreaterThanOrEqualTo(0));
        When(x => x.TaxAmount.HasValue, () => RuleFor(x => x.TaxAmount!.Value).GreaterThanOrEqualTo(0));
    }
}

