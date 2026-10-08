using Domain.Catalogs.Entities;
using Domain.Catalogs.Events;
using Domain.Catalogs.ValueObjects;
using NUnit.Framework;

namespace Tests.Unit.Domain;

[TestFixture]
public class CompanyTests
{
    [Test]
    public void Create_RaisesCompanyCreatedEvent()
    {
        var now = DateTime.UtcNow;
        var company = Company.Create(
            Guid.NewGuid(),
            CatalogCode.Create("EMP001"),
            CatalogDescription.Create("Mi Minería S.A."),
            now);

        Assert.That(company.Code.Value, Is.EqualTo("EMP001"));
        Assert.That(company.DomainEvents, Has.One.InstanceOf<CompanyCreatedEvent>());
    }

    [Test]
    public void ChangeCode_RaisesCompanyUpdatedEvent()
    {
        var company = Company.Create(
            Guid.NewGuid(),
            CatalogCode.Create("EMP001"),
            CatalogDescription.Create("Mi Minería S.A."),
            DateTime.UtcNow);
        company.ClearDomainEvents();

        company.ChangeCode(CatalogCode.Create("EMP002"), DateTime.UtcNow);

        Assert.That(company.Code.Value, Is.EqualTo("EMP002"));
        Assert.That(company.DomainEvents, Has.One.InstanceOf<CompanyUpdatedEvent>());
    }

    [Test]
    public void ChangeDescription_RaisesCompanyUpdatedEvent()
    {
        var company = Company.Create(
            Guid.NewGuid(),
            CatalogCode.Create("EMP001"),
            CatalogDescription.Create("Mi Minería S.A."),
            DateTime.UtcNow);
        company.ClearDomainEvents();

        company.ChangeDescription(CatalogDescription.Create("Otra descripción"), DateTime.UtcNow);

        Assert.That(company.Description.Value, Is.EqualTo("Otra descripción"));
        Assert.That(company.DomainEvents, Has.One.InstanceOf<CompanyUpdatedEvent>());
    }

    [Test]
    public void MarkDeleted_RaisesCompanyDeletedEvent()
    {
        var company = Company.Create(
            Guid.NewGuid(),
            CatalogCode.Create("EMP001"),
            CatalogDescription.Create("Mi Minería S.A."),
            DateTime.UtcNow);
        company.ClearDomainEvents();

        company.MarkDeleted(DateTime.UtcNow);

        Assert.That(company.DomainEvents, Has.One.InstanceOf<CompanyDeletedEvent>());
    }
}
