using MediatR;

namespace VOEConsulting.Flame.OrderManagement.Application.Abstractions.CQRS;

public interface IQuery<out TResponse> : IRequest<TResponse>
    where TResponse : notnull { }

