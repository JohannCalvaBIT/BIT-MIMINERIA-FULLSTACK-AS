namespace Domain.Catalogs.Events;

public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}
