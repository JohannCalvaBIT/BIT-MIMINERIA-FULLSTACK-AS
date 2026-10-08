using Domain.Catalogs.Entities;
using Domain.Catalogs.ValueObjects;
using Infrastructure.Catalogs.Persistence;
using Infrastructure.Catalogs.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Tests.Integration;

[TestFixture]
[Explicit("Requiere Testcontainers/SQL Server local. No ejecutable en sandbox sin Docker.")]
public class CatalogIntegrationTests
{
    private CatalogDbContext _dbContext = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseSqlServer("Server=localhost,1433;Database=MiMineriaCatalogs;User Id=sa;Password=Local_Dev_Only_ChangeMe;TrustServerCertificate=True;Encrypt=False")
            .Options;
        _dbContext = new CatalogDbContext(options);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
    }

    [Test]
    public async Task CompanyRepository_CreateReadUpdateDelete_WorksAgainstSql()
    {
        var repository = new CompanyRepository(_dbContext);
        var company = Company.Create(
            Guid.NewGuid(),
            CatalogCode.Create("INT001"),
            CatalogDescription.Create("Integration company"),
            DateTime.UtcNow);

        await repository.AddAsync(company);
        await _dbContext.SaveChangesAsync();

        var found = await repository.GetByCodeAsync("int001");
        Assert.That(found, Is.Not.Null);
        Assert.That(found!.Id, Is.EqualTo(company.Id));

        found.ChangeDescription(CatalogDescription.Create("Updated"), DateTime.UtcNow);
        repository.Update(found);
        await _dbContext.SaveChangesAsync();

        var updated = await repository.GetByIdAsync(company.Id);
        Assert.That(updated!.Description.Value, Is.EqualTo("Updated"));

        repository.Remove(updated);
        await _dbContext.SaveChangesAsync();

        Assert.That(await repository.GetByIdAsync(company.Id), Is.Null);
    }
}
