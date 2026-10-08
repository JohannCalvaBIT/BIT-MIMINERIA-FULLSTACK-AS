using Application.Catalogs.Commands;
using Application.Catalogs.DTOs;
using Application.Catalogs.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Catalogs.Controllers;

[ApiController]
[Route("api/catalogs")]
[Authorize(Roles = "GlobalAdmin")]
public sealed class CatalogController : ControllerBase
{
    private readonly IMediator _mediator;

    public CatalogController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("companies")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> CreateCompany(CreateCompanyDto dto, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(new CreateCompanyCommand(dto.Code, dto.Description), cancellationToken);
        return Created($"/api/catalogs/companies/{id}", id);
    }

    [HttpPut("companies/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateCompany(Guid id, UpdateCatalogDto dto, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateCompanyCommand(id, dto.Code, dto.Description), cancellationToken);
        return NoContent();
    }

    [HttpDelete("companies/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteCompany(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteCompanyCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpGet("companies")]
    public async Task<ActionResult<PagedResult<CatalogItemDto>>> GetCompanies(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 10,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _mediator.Send(new GetCompaniesQuery(skip, take), cancellationToken));
    }

    [HttpGet("companies/search")]
    public async Task<ActionResult<PagedResult<CatalogItemDto>>> SearchCompanies(
        [FromQuery] string? code,
        [FromQuery] string? description,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 10,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _mediator.Send(new SearchCompaniesQuery(code, description, skip, take), cancellationToken));
    }

    [HttpPost("formats")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> CreateFormat(CreateFormatDto dto, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(new CreateFormatCommand(dto.Code, dto.Description), cancellationToken);
        return Created($"/api/catalogs/formats/{id}", id);
    }

    [HttpPut("formats/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateFormat(Guid id, UpdateCatalogDto dto, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateFormatCommand(id, dto.Code, dto.Description), cancellationToken);
        return NoContent();
    }

    [HttpDelete("formats/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteFormat(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteFormatCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpGet("formats")]
    public async Task<ActionResult<PagedResult<CatalogItemDto>>> GetFormats(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 10,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _mediator.Send(new GetFormatsQuery(skip, take), cancellationToken));
    }

    [HttpGet("formats/search")]
    public async Task<ActionResult<PagedResult<CatalogItemDto>>> SearchFormats(
        [FromQuery] string? code,
        [FromQuery] string? description,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 10,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _mediator.Send(new SearchFormatsQuery(code, description, skip, take), cancellationToken));
    }

    [HttpPost("disciplines")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> CreateDiscipline(CreateDisciplineDto dto, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(new CreateDisciplineCommand(dto.Code, dto.Description), cancellationToken);
        return Created($"/api/catalogs/disciplines/{id}", id);
    }

    [HttpPut("disciplines/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateDiscipline(Guid id, UpdateCatalogDto dto, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateDisciplineCommand(id, dto.Code, dto.Description), cancellationToken);
        return NoContent();
    }

    [HttpDelete("disciplines/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteDiscipline(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteDisciplineCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpGet("disciplines")]
    public async Task<ActionResult<PagedResult<CatalogItemDto>>> GetDisciplines(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 10,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _mediator.Send(new GetDisciplinesQuery(skip, take), cancellationToken));
    }

    [HttpGet("disciplines/search")]
    public async Task<ActionResult<PagedResult<CatalogItemDto>>> SearchDisciplines(
        [FromQuery] string? code,
        [FromQuery] string? description,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 10,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _mediator.Send(new SearchDisciplinesQuery(code, description, skip, take), cancellationToken));
    }
}

public sealed record CreateCompanyDto(string Code, string Description);
public sealed record CreateFormatDto(string Code, string Description);
public sealed record CreateDisciplineDto(string Code, string Description);
public sealed record UpdateCatalogDto(string Code, string Description);
