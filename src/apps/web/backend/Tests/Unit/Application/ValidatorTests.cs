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
}
