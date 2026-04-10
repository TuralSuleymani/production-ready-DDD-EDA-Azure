using CSharpFunctionalExtensions;

namespace VOEConsulting.Flame.OrderManagement.Application.Orders.Commands.CancelOrder;

public sealed record CancelOrderCommand(string TenantId, string OrderId) : ICommand<UnitResult<DomainError>>;

