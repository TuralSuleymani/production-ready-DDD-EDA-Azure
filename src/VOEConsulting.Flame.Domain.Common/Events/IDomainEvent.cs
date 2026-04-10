using MediatR;

namespace VOEConsulting.Flame.Domain.Common.Events;

public interface IDomainEvent : INotification
{
    int Version { get; }
    string AggregateType { get; }
    string EventType { get; }
    Guid Id { get; }
    DateTimeOffset OccurredOnUtc { get; }
    Guid AggregateId { get; }
    string? TraceInfo { get; }
}
