using Domain.Catalogs.Entities;
using MediatR;

namespace Application.Catalogs.Commands;

public sealed record CreateCompanyCommand(string Code, string Description) : IRequest<Guid>;

public sealed record UpdateCompanyCommand(Guid Id, string Code, string Description) : IRequest;

public sealed record DeleteCompanyCommand(Guid Id) : IRequest;

public sealed record CreateFormatCommand(string Code, string Description) : IRequest<Guid>;

public sealed record UpdateFormatCommand(Guid Id, string Code, string Description) : IRequest;

public sealed record DeleteFormatCommand(Guid Id) : IRequest;

public sealed record CreateDisciplineCommand(string Code, string Description) : IRequest<Guid>;

public sealed record UpdateDisciplineCommand(Guid Id, string Code, string Description) : IRequest;

public sealed record DeleteDisciplineCommand(Guid Id) : IRequest;
