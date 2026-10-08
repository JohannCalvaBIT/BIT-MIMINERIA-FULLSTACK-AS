using Application.Catalogs.Abstractions;
using Domain.Catalogs.Entities;
using Domain.Catalogs.Exceptions;
using Domain.Catalogs.ValueObjects;
using MediatR;

namespace Application.Catalogs.Commands;

public sealed class CreateFormatCommandHandler : IRequestHandler<CreateFormatCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateFormatCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateFormatCommand request, CancellationToken cancellationToken)
    {
        if (await _unitOfWork.Formats.GetByCodeAsync(request.Code, cancellationToken) is not null)
        {
            throw new DuplicateCodeException(request.Code);
        }

        if (await _unitOfWork.Formats.GetByDescriptionAsync(request.Description, cancellationToken) is not null)
        {
            throw new DuplicateDescriptionException(request.Description);
        }

        var format = Format.Create(
            Guid.NewGuid(),
            CatalogCode.Create(request.Code),
            CatalogDescription.Create(request.Description),
            DateTime.UtcNow);

        await _unitOfWork.Formats.AddAsync(format, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return format.Id;
    }
}

public sealed class UpdateFormatCommandHandler : IRequestHandler<UpdateFormatCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateFormatCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateFormatCommand request, CancellationToken cancellationToken)
    {
        var format = await _unitOfWork.Formats.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Format {request.Id} not found.");

        var inUse = await _unitOfWork.Formats.GetUsageCountAsync(request.Id, cancellationToken) > 0;

        if (!inUse)
        {
            var code = CatalogCode.Create(request.Code);
            if (!format.Code.Equals(code) &&
                await _unitOfWork.Formats.GetByCodeAsync(request.Code, cancellationToken) is not null)
            {
                throw new DuplicateCodeException(request.Code);
            }

            format.ChangeCode(code, DateTime.UtcNow);
        }

        var description = CatalogDescription.Create(request.Description);
        if (!format.Description.Equals(description) &&
            await _unitOfWork.Formats.GetByDescriptionAsync(request.Description, cancellationToken) is not null)
        {
            throw new DuplicateDescriptionException(request.Description);
        }

        format.ChangeDescription(description, DateTime.UtcNow);
        _unitOfWork.Formats.Update(format);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public sealed class DeleteFormatCommandHandler : IRequestHandler<DeleteFormatCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteFormatCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteFormatCommand request, CancellationToken cancellationToken)
    {
        var format = await _unitOfWork.Formats.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Format {request.Id} not found.");

        var usageCount = await _unitOfWork.Formats.GetUsageCountAsync(request.Id, cancellationToken);
        if (usageCount > 0)
        {
            throw new CatalogInUseException(usageCount);
        }

        format.MarkDeleted(DateTime.UtcNow);
        _unitOfWork.Formats.Remove(format);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
