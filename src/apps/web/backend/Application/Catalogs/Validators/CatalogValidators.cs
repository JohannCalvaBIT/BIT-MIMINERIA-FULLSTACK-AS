using Application.Catalogs.Commands;
using Domain.Catalogs.Interfaces;
using FluentValidation;

namespace Application.Catalogs.Validators;

public sealed class CreateCompanyCommandValidator : AbstractValidator<CreateCompanyCommand>
{
    public CreateCompanyCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(250);
    }
}

public sealed class UpdateCompanyCommandValidator : AbstractValidator<UpdateCompanyCommand>
{
    public UpdateCompanyCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(250);
    }
}

public sealed class DeleteCompanyCommandValidator : AbstractValidator<DeleteCompanyCommand>
{
    public DeleteCompanyCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public sealed class CreateFormatCommandValidator : AbstractValidator<CreateFormatCommand>
{
    public CreateFormatCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(250);
    }
}

public sealed class UpdateFormatCommandValidator : AbstractValidator<UpdateFormatCommand>
{
    public UpdateFormatCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(250);
    }
}

public sealed class DeleteFormatCommandValidator : AbstractValidator<DeleteFormatCommand>
{
    public DeleteFormatCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public sealed class CreateDisciplineCommandValidator : AbstractValidator<CreateDisciplineCommand>
{
    public CreateDisciplineCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(250);
    }
}

public sealed class UpdateDisciplineCommandValidator : AbstractValidator<UpdateDisciplineCommand>
{
    public UpdateDisciplineCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(250);
    }
}

public sealed class DeleteDisciplineCommandValidator : AbstractValidator<DeleteDisciplineCommand>
{
    public DeleteDisciplineCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
