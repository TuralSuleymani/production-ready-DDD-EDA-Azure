using MediatR;

namespace VOEConsulting.Flame.OrderManagement.Application.Abstractions.CQRS;

public interface ICommand<out TResponse> : IRequest<TResponse>
    where TResponse : notnull { }

public interface ICommand : ICommand<Unit> { }

