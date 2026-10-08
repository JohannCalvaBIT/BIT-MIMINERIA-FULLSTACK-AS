
using Domain.Catalogs.Entities;
using Domain.Catalogs.Exceptions;
using Domain.Catalogs.ValueObjects;
using MediatR;

namespace Application.Catalogs.Commands;

public sealed class CreateCompanyCommandHandler : IRequestHandler<CreateCompanyCommand, Guid>
{
    private readonly Domain.Catalogs.Interfaces.IUnitOfWork _unitOfWork;

    public CreateCompanyCommandHandler(Domain.Catalogs.Interfaces.IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateCompanyCommand request, CancellationToken cancellationToken)
    {
        if (await _unitOfWork.Companies.GetByCodeAsync(request.Code, cancellationToken) is not null)
        {
            throw new DuplicateCodeException(request.Code);
        }

        if (await _unitOfWork.Companies.GetByDescriptionAsync(request.Description, cancellationToken) is not null)
        {
            throw new DuplicateDescriptionException(request.Description);
        }

        var company = Company.Create(
            Guid.NewGuid(),
            CatalogCode.Create(request.Code),
            CatalogDescription.Create(request.Description),
            DateTime.UtcNow);

        await _unitOfWork.Companies.AddAsync(company, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return company.Id;
    }
}

public sealed class UpdateCompanyCommandHandler : IRequestHandler<UpdateCompanyCommand>
{
    private readonly Domain.Catalogs.Interfaces.IUnitOfWork _unitOfWork;

    public UpdateCompanyCommandHandler(Domain.Catalogs.Interfaces.IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = await _unitOfWork.Companies.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Company {request.Id} not found.");

        var inUse = await _unitOfWork.Companies.GetUsageCountAsync(request.Id, cancellationToken) > 0;

        if (!inUse)
        {
            var code = CatalogCode.Create(request.Code);
            if (!company.Code.Equals(code) &&
                await _unitOfWork.Companies.GetByCodeAsync(request.Code, cancellationToken) is not null)
            {
                throw new DuplicateCodeException(request.Code);
            }

            company.ChangeCode(code, DateTime.UtcNow);
        }

        var description = CatalogDescription.Create(request.Description);
        if (!company.Description.Equals(description) &&
            await _unitOfWork.Companies.GetByDescriptionAsync(request.Description, cancellationToken) is not null)
        {
            throw new DuplicateDescriptionException(request.Description);
        }

        company.ChangeDescription(description, DateTime.UtcNow);
        _unitOfWork.Companies.Update(company);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public sealed class DeleteCompanyCommandHandler : IRequestHandler<DeleteCompanyCommand>
{
    private readonly Domain.Catalogs.Interfaces.IUnitOfWork _unitOfWork;

    public DeleteCompanyCommandHandler(Domain.Catalogs.Interfaces.IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = await _unitOfWork.Companies.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Company {request.Id} not found.");

        var usageCount = await _unitOfWork.Companies.GetUsageCountAsync(request.Id, cancellationToken);
        if (usageCount > 0)
        {
            throw new CatalogInUseException(usageCount);
        }

        company.MarkDeleted(DateTime.UtcNow);
        _unitOfWork.Companies.Remove(company);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
