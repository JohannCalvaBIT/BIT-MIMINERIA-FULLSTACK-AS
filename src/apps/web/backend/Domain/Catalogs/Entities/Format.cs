using Domain.Catalogs.Events;
using Domain.Catalogs.Interfaces;
using Domain.Catalogs.ValueObjects;

namespace Domain.Catalogs.Entities;

public sealed class Format : IAggregateRoot, ICatalogEntity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    private Format(Guid id, CatalogCode code, CatalogDescription description, DateTime createdAt)
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

    public static Format Create(Guid id, CatalogCode code, CatalogDescription description, DateTime now)
    {
        var format = new Format(id, code, description, now);
        format._domainEvents.Add(new FormatCreatedEvent(id, code.Value, description.Value, now));
        return format;
    }

    public void ChangeCode(CatalogCode code, DateTime now)
    {
        if (Code.Equals(code))
        {
            return;
        }

        Code = code;
        MarkUpdated(now);
    }

    public void ChangeDescription(CatalogDescription description, DateTime now)
    {
        if (Description.Equals(description))
        {
            return;
        }

        Description = description;
        MarkUpdated(now);
    }

    public void MarkDeleted(DateTime now)
    {
        _domainEvents.Add(new FormatDeletedEvent(Id, Code.Value, now));
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    private void MarkUpdated(DateTime now)
    {
        ModifiedAt = now;
        _domainEvents.Add(new FormatUpdatedEvent(Id, Code.Value, Description.Value, now));
    }
}
