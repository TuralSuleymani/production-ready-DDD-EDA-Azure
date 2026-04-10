using FluentValidation;

namespace VOEConsulting.Flame.OrderManagement.Application.Orders.Commands.MarkShipped;

public sealed class MarkShippedCommandValidator : AbstractValidator<MarkShippedCommand>
{
    public MarkShippedCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.OrderId).NotEmpty();
    }
}

