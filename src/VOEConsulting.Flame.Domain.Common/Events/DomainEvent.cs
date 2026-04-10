namespace VOEConsulting.Flame.Domain.Common.Events;

public abstract class DomainEvent : IDomainEvent
{
    public int Version { get; set; } = 1;
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AggregateId { get; set; }
    public DateTimeOffset OccurredOnUtc { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string AggregateType { get; set; } = string.Empty;
    public string? TraceInfo { get; set; }

    protected DomainEvent(Guid aggregateId, DateTimeOffset occurredOnUtc, string aggregateTypeName)
    {
        AggregateId = aggregateId;
        OccurredOnUtc = occurredOnUtc;
        AggregateType = aggregateTypeName;
        EventType = $"{aggregateTypeName}.{GetType().Name}";
    }
}
