namespace VOEConsulting.Flame.Domain.Common;

public interface IAggregateRoot
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    IReadOnlyCollection<IDomainEvent> PopDomainEvents();
    void ClearEvents();
}
