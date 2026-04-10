using VOEConsulting.Flame.OrderManagement.Domain.Orders;

namespace VOEConsulting.Flame.OrderManagement.Application.Abstractions;

public sealed record OrderPersistenceState(Order Order, string? ETag);
