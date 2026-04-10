using CSharpFunctionalExtensions;

namespace VOEConsulting.Flame.OrderManagement.Application.Orders.Commands.MarkShipped;

public sealed record MarkShippedCommand(string TenantId, string OrderId) : ICommand<UnitResult<DomainError>>;

