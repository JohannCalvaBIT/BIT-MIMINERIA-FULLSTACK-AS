namespace Application.Catalogs.DTOs;

public sealed record CatalogItemDto(Guid Id, string Code, string Description, bool IsInUse, DateTime CreatedAt, DateTime ModifiedAt);

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Total);
