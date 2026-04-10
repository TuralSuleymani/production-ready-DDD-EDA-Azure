namespace VOEConsulting.Flame.Domain.Common;

public abstract class AggregateRoot<TModel> : Entity<TModel>, IAggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = new();

    protected AggregateRoot(Id<TModel> id)
        : base(id)
    {
    }

    protected AggregateRoot(Id<TModel> id, DateTimeOffset createdAtUtc, DateTimeOffset lastModifiedAtUtc)
        : base(id, createdAtUtc, lastModifiedAtUtc)
    {
    }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public IReadOnlyCollection<IDomainEvent> PopDomainEvents()
    {
        var copy = _domainEvents.ToList();
        ClearEvents();
        return copy;
    }

    public void ClearEvents() => _domainEvents.Clear();

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }
}
