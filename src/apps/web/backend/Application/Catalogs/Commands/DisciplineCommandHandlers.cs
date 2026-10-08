
using Domain.Catalogs.Entities;
using Domain.Catalogs.Exceptions;
using Domain.Catalogs.ValueObjects;
using MediatR;

namespace Application.Catalogs.Commands;

public sealed class CreateDisciplineCommandHandler : IRequestHandler<CreateDisciplineCommand, Guid>
{
    private readonly Domain.Catalogs.Interfaces.IUnitOfWork _unitOfWork;

    public CreateDisciplineCommandHandler(Domain.Catalogs.Interfaces.IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateDisciplineCommand request, CancellationToken cancellationToken)
    {
        if (await _unitOfWork.Disciplines.GetByCodeAsync(request.Code, cancellationToken) is not null)
        {
            throw new DuplicateCodeException(request.Code);
        }

        if (await _unitOfWork.Disciplines.GetByDescriptionAsync(request.Description, cancellationToken) is not null)
        {
            throw new DuplicateDescriptionException(request.Description);
        }

        var discipline = Discipline.Create(
            Guid.NewGuid(),
            CatalogCode.Create(request.Code),
            CatalogDescription.Create(request.Description),
            DateTime.UtcNow);

        await _unitOfWork.Disciplines.AddAsync(discipline, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return discipline.Id;
    }
}

public sealed class UpdateDisciplineCommandHandler : IRequestHandler<UpdateDisciplineCommand>
{
    private readonly Domain.Catalogs.Interfaces.IUnitOfWork _unitOfWork;

    public UpdateDisciplineCommandHandler(Domain.Catalogs.Interfaces.IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateDisciplineCommand request, CancellationToken cancellationToken)
    {
        var discipline = await _unitOfWork.Disciplines.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Discipline {request.Id} not found.");

        var inUse = await _unitOfWork.Disciplines.GetUsageCountAsync(request.Id, cancellationToken) > 0;

        if (!inUse)
        {
            var code = CatalogCode.Create(request.Code);
            if (!discipline.Code.Equals(code) &&
                await _unitOfWork.Disciplines.GetByCodeAsync(request.Code, cancellationToken) is not null)
            {
                throw new DuplicateCodeException(request.Code);
            }

            discipline.ChangeCode(code, DateTime.UtcNow);
        }

        var description = CatalogDescription.Create(request.Description);
        if (!discipline.Description.Equals(description) &&
            await _unitOfWork.Disciplines.GetByDescriptionAsync(request.Description, cancellationToken) is not null)
        {
            throw new DuplicateDescriptionException(request.Description);
        }

        discipline.ChangeDescription(description, DateTime.UtcNow);
        _unitOfWork.Disciplines.Update(discipline);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public sealed class DeleteDisciplineCommandHandler : IRequestHandler<DeleteDisciplineCommand>
{
    private readonly Domain.Catalogs.Interfaces.IUnitOfWork _unitOfWork;

    public DeleteDisciplineCommandHandler(Domain.Catalogs.Interfaces.IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteDisciplineCommand request, CancellationToken cancellationToken)
    {
        var discipline = await _unitOfWork.Disciplines.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Discipline {request.Id} not found.");

        var usageCount = await _unitOfWork.Disciplines.GetUsageCountAsync(request.Id, cancellationToken);
        if (usageCount > 0)
        {
            throw new CatalogInUseException(usageCount);
        }

        discipline.MarkDeleted(DateTime.UtcNow);
        _unitOfWork.Disciplines.Remove(discipline);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
