using Application.Catalogs.DTOs;
using MediatR;

namespace Application.Catalogs.Queries;

public sealed record GetCompaniesQuery(int Skip = 0, int Take = 10) : IRequest<PagedResult<CatalogItemDto>>;
public sealed record SearchCompaniesQuery(string? Code, string? Description, int Skip = 0, int Take = 10)
    : IRequest<PagedResult<CatalogItemDto>>;

public sealed record GetFormatsQuery(int Skip = 0, int Take = 10) : IRequest<PagedResult<CatalogItemDto>>;
public sealed record SearchFormatsQuery(string? Code, string? Description, int Skip = 0, int Take = 10)
    : IRequest<PagedResult<CatalogItemDto>>;

public sealed record GetDisciplinesQuery(int Skip = 0, int Take = 10) : IRequest<PagedResult<CatalogItemDto>>;
public sealed record SearchDisciplinesQuery(string? Code, string? Description, int Skip = 0, int Take = 10)
    : IRequest<PagedResult<CatalogItemDto>>;
