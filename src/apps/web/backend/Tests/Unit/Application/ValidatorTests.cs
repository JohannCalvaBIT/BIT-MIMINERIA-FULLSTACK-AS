using Application.Catalogs.Commands;
using Application.Catalogs.Validators;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Tests.Unit.Application;

[TestFixture]
public class ValidatorTests
{
    [Test]
    public void CreateCompanyCommandValidator_ValidInput_Passes()
    {
        var validator = new CreateCompanyCommandValidator();
        var result = validator.TestValidate(new CreateCompanyCommand("EMP001", "Mi Minería S.A."));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void CreateCompanyCommandValidator_TooLongCode_Fails()
    {
        var validator = new CreateCompanyCommandValidator();
        var result = validator.TestValidate(new CreateCompanyCommand(new string('a', 151), "Descripción"));
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Test]
    public void CreateCompanyCommandValidator_EmptyDescription_Fails()
    {
        var validator = new CreateCompanyCommandValidator();
        var result = validator.TestValidate(new CreateCompanyCommand("EMP001", ""));
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Test]
    public void UpdateCompanyCommandValidator_EmptyId_Fails()
    {
        var validator = new UpdateCompanyCommandValidator();
        var result = validator.TestValidate(new UpdateCompanyCommand(Guid.Empty, "EMP001", "Descripción"));
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Test]
    public void DeleteCompanyCommandValidator_EmptyId_Fails()
    {
        var validator = new DeleteCompanyCommandValidator();
        var result = validator.TestValidate(new DeleteCompanyCommand(Guid.Empty));
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Test]
    public void CreateFormatCommandValidator_ValidInput_Passes()
    {
        var validator = new CreateFormatCommandValidator();
        var result = validator.TestValidate(new CreateFormatCommand("FMT001", "Video"));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void UpdateDisciplineCommandValidator_TooLongDescription_Fails()
    {
        var validator = new UpdateDisciplineCommandValidator();
        var result = validator.TestValidate(
            new UpdateDisciplineCommand(Guid.NewGuid(), "DSC001", new string('d', 251)));
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }
}
