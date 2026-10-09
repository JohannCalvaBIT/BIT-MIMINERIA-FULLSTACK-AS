namespace Domain.Catalogs.Events;

public abstract record CatalogDomainEvent(Guid EntityId, DateTime OccurredAt) : IDomainEvent;

public sealed record CompanyCreatedEvent(Guid EntityId, string Code, string Description, DateTime OccurredAt)
    : CatalogDomainEvent(EntityId, OccurredAt);

public sealed record CompanyUpdatedEvent(
    Guid EntityId,
    string OldCode,
    string OldDescription,
    string Code,
    string Description,
    DateTime OccurredAt)
    : CatalogDomainEvent(EntityId, OccurredAt);

public sealed record CompanyDeletedEvent(Guid EntityId, string Code, DateTime OccurredAt)
    : CatalogDomainEvent(EntityId, OccurredAt);

public sealed record FormatCreatedEvent(Guid EntityId, string Code, string Description, DateTime OccurredAt)
    : CatalogDomainEvent(EntityId, OccurredAt);

public sealed record FormatUpdatedEvent(
    Guid EntityId,
    string OldCode,
    string OldDescription,
    string Code,
    string Description,
    DateTime OccurredAt)
    : CatalogDomainEvent(EntityId, OccurredAt);

public sealed record FormatDeletedEvent(Guid EntityId, string Code, DateTime OccurredAt)
    : CatalogDomainEvent(EntityId, OccurredAt);

public sealed record DisciplineCreatedEvent(Guid EntityId, string Code, string Description, DateTime OccurredAt)
    : CatalogDomainEvent(EntityId, OccurredAt);

public sealed record DisciplineUpdatedEvent(
    Guid EntityId,
    string OldCode,
    string OldDescription,
    string Code,
    string Description,
    DateTime OccurredAt)
    : CatalogDomainEvent(EntityId, OccurredAt);

public sealed record DisciplineDeletedEvent(Guid EntityId, string Code, DateTime OccurredAt)
    : CatalogDomainEvent(EntityId, OccurredAt);
