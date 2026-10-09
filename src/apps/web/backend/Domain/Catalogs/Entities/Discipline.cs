using Domain.Catalogs.Events;
using Domain.Catalogs.Interfaces;
using Domain.Catalogs.ValueObjects;

namespace Domain.Catalogs.Entities;

public sealed class Discipline : IAggregateRoot, ICatalogEntity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    private Discipline(Guid id, CatalogCode code, CatalogDescription description, DateTime createdAt)
    {
        Id = id;
        Code = code;
        Description = description;
        CreatedAt = createdAt;
        ModifiedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public CatalogCode Code { get; private set; }
    public CatalogDescription Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime ModifiedAt { get; private set; }
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public static Discipline Create(Guid id, CatalogCode code, CatalogDescription description, DateTime now)
    {
        var discipline = new Discipline(id, code, description, now);
        discipline._domainEvents.Add(new DisciplineCreatedEvent(id, code.Value, description.Value, now));
        return discipline;
    }

    public void ChangeCode(CatalogCode code, DateTime now)
    {
        if (Code.Equals(code))
        {
            return;
        }

        var oldCode = Code;
        var oldDescription = Description;
        Code = code;
        MarkUpdated(oldCode, oldDescription, now);
    }

    public void ChangeDescription(CatalogDescription description, DateTime now)
    {
        if (Description.Equals(description))
        {
            return;
        }

        var oldCode = Code;
        var oldDescription = Description;
        Description = description;
        MarkUpdated(oldCode, oldDescription, now);
    }

    public void MarkDeleted(DateTime now)
    {
        _domainEvents.Add(new DisciplineDeletedEvent(Id, Code.Value, now));
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    private void MarkUpdated(CatalogCode oldCode, CatalogDescription oldDescription, DateTime now)
    {
        ModifiedAt = now;
        _domainEvents.Add(new DisciplineUpdatedEvent(
            Id,
            oldCode.Value,
            oldDescription.Value,
            Code.Value,
            Description.Value,
            now));
    }
}
