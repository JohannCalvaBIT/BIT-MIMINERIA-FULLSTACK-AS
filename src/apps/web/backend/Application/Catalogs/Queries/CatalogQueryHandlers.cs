using Application.Catalogs.Abstractions;
using Application.Catalogs.DTOs;
using MediatR;

namespace Application.Catalogs.Queries;

public sealed class GetCompaniesQueryHandler : IRequestHandler<GetCompaniesQuery, PagedResult<CatalogItemDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCompaniesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<CatalogItemDto>> Handle(GetCompaniesQuery request, CancellationToken cancellationToken)
    {
        var items = await _unitOfWork.Companies.GetPagedAsync(request.Skip, request.Take, cancellationToken);
        var total = await _unitOfWork.Companies.CountAsync(cancellationToken);
        var dtos = await ToDtosAsync(items, cancellationToken);
        return new PagedResult<CatalogItemDto>(dtos, total);
    }

    private async Task<IReadOnlyList<CatalogItemDto>> ToDtosAsync(
        IReadOnlyList<Domain.Catalogs.Entities.Company> items,
        CancellationToken cancellationToken)
    {
        var result = new List<CatalogItemDto>(items.Count);
        foreach (var item in items)
        {
            var inUse = await _unitOfWork.Companies.GetUsageCountAsync(item.Id, cancellationToken) > 0;
            result.Add(new CatalogItemDto(item.Id, item.Code.Value, item.Description.Value, inUse, item.CreatedAt, item.ModifiedAt));
        }

        return result;
    }
}

public sealed class SearchCompaniesQueryHandler : IRequestHandler<SearchCompaniesQuery, PagedResult<CatalogItemDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public SearchCompaniesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<CatalogItemDto>> Handle(SearchCompaniesQuery request, CancellationToken cancellationToken)
    {
        var items = await _unitOfWork.Companies.SearchAsync(
            request.Code, request.Description, request.Skip, request.Take, cancellationToken);
        var total = await _unitOfWork.Companies.CountSearchAsync(request.Code, request.Description, cancellationToken);
        var result = new List<CatalogItemDto>(items.Count);
        foreach (var item in items)
        {
            var inUse = await _unitOfWork.Companies.GetUsageCountAsync(item.Id, cancellationToken) > 0;
            result.Add(new CatalogItemDto(item.Id, item.Code.Value, item.Description.Value, inUse, item.CreatedAt, item.ModifiedAt));
        }

        return new PagedResult<CatalogItemDto>(result, total);
    }
}

public sealed class GetFormatsQueryHandler : IRequestHandler<GetFormatsQuery, PagedResult<CatalogItemDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetFormatsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<CatalogItemDto>> Handle(GetFormatsQuery request, CancellationToken cancellationToken)
    {
        var items = await _unitOfWork.Formats.GetPagedAsync(request.Skip, request.Take, cancellationToken);
        var total = await _unitOfWork.Formats.CountAsync(cancellationToken);
        var result = new List<CatalogItemDto>(items.Count);
        foreach (var item in items)
        {
            var inUse = await _unitOfWork.Formats.GetUsageCountAsync(item.Id, cancellationToken) > 0;
            result.Add(new CatalogItemDto(item.Id, item.Code.Value, item.Description.Value, inUse, item.CreatedAt, item.ModifiedAt));
        }

        return new PagedResult<CatalogItemDto>(result, total);
    }
}

public sealed class SearchFormatsQueryHandler : IRequestHandler<SearchFormatsQuery, PagedResult<CatalogItemDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public SearchFormatsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<CatalogItemDto>> Handle(SearchFormatsQuery request, CancellationToken cancellationToken)
    {
        var items = await _unitOfWork.Formats.SearchAsync(
            request.Code, request.Description, request.Skip, request.Take, cancellationToken);
        var total = await _unitOfWork.Formats.CountSearchAsync(request.Code, request.Description, cancellationToken);
        var result = new List<CatalogItemDto>(items.Count);
        foreach (var item in items)
        {
            var inUse = await _unitOfWork.Formats.GetUsageCountAsync(item.Id, cancellationToken) > 0;
            result.Add(new CatalogItemDto(item.Id, item.Code.Value, item.Description.Value, inUse, item.CreatedAt, item.ModifiedAt));
        }

        return new PagedResult<CatalogItemDto>(result, total);
    }
}

public sealed class GetDisciplinesQueryHandler : IRequestHandler<GetDisciplinesQuery, PagedResult<CatalogItemDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetDisciplinesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<CatalogItemDto>> Handle(GetDisciplinesQuery request, CancellationToken cancellationToken)
    {
        var items = await _unitOfWork.Disciplines.GetPagedAsync(request.Skip, request.Take, cancellationToken);
        var total = await _unitOfWork.Disciplines.CountAsync(cancellationToken);
        var result = new List<CatalogItemDto>(items.Count);
        foreach (var item in items)
        {
            var inUse = await _unitOfWork.Disciplines.GetUsageCountAsync(item.Id, cancellationToken) > 0;
            result.Add(new CatalogItemDto(item.Id, item.Code.Value, item.Description.Value, inUse, item.CreatedAt, item.ModifiedAt));
        }

        return new PagedResult<CatalogItemDto>(result, total);
    }
}

public sealed class SearchDisciplinesQueryHandler : IRequestHandler<SearchDisciplinesQuery, PagedResult<CatalogItemDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public SearchDisciplinesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<CatalogItemDto>> Handle(SearchDisciplinesQuery request, CancellationToken cancellationToken)
    {
        var items = await _unitOfWork.Disciplines.SearchAsync(
            request.Code, request.Description, request.Skip, request.Take, cancellationToken);
        var total = await _unitOfWork.Disciplines.CountSearchAsync(request.Code, request.Description, cancellationToken);
        var result = new List<CatalogItemDto>(items.Count);
        foreach (var item in items)
        {
            var inUse = await _unitOfWork.Disciplines.GetUsageCountAsync(item.Id, cancellationToken) > 0;
            result.Add(new CatalogItemDto(item.Id, item.Code.Value, item.Description.Value, inUse, item.CreatedAt, item.ModifiedAt));
        }

        return new PagedResult<CatalogItemDto>(result, total);
    }
}
