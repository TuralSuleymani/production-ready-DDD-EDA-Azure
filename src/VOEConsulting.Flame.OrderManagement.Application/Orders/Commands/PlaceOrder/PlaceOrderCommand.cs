using CSharpFunctionalExtensions;

namespace VOEConsulting.Flame.OrderManagement.Application.Orders.Commands.PlaceOrder;

public sealed record PlaceOrderCommand(
    string TenantId,
    string OrderId,
    string CustomerId,
    decimal TotalAmount,
    string Currency,
    decimal? SubtotalAmount = null,
    decimal? DiscountAmount = null,
    decimal? TaxAmount = null) : ICommand<UnitResult<DomainError>>;

