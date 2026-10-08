using Domain.Catalogs.Interfaces;
using Infrastructure.Catalogs.Repositories;

namespace Infrastructure.Catalogs.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly CatalogDbContext _dbContext;
    private readonly Lazy<ICompanyRepository> _companies;
    private readonly Lazy<IFormatRepository> _formats;
    private readonly Lazy<IDisciplineRepository> _disciplines;

    public UnitOfWork(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
        _companies = new Lazy<ICompanyRepository>(() => new CompanyRepository(_dbContext));
        _formats = new Lazy<IFormatRepository>(() => new FormatRepository(_dbContext));
        _disciplines = new Lazy<IDisciplineRepository>(() => new DisciplineRepository(_dbContext));
    }

    public ICompanyRepository Companies => _companies.Value;
    public IFormatRepository Formats => _formats.Value;
    public IDisciplineRepository Disciplines => _disciplines.Value;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);

    public ValueTask DisposeAsync() => _dbContext.DisposeAsync();
}
