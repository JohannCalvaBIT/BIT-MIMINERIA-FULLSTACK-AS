using Domain.Catalogs.Interfaces;
using Infrastructure.Catalogs.Auditing;
using Infrastructure.Catalogs.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Catalogs.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly CatalogDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly CatalogChangeAuditedHandler _auditedHandler;
    private readonly Lazy<ICompanyRepository> _companies;
    private readonly Lazy<IFormatRepository> _formats;
    private readonly Lazy<IDisciplineRepository> _disciplines;

    public UnitOfWork(
        CatalogDbContext dbContext,
        ICurrentUserService currentUser,
        CatalogChangeAuditedHandler auditedHandler)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _auditedHandler = auditedHandler;
        _companies = new Lazy<ICompanyRepository>(() => new CompanyRepository(_dbContext));
        _formats = new Lazy<IFormatRepository>(() => new FormatRepository(_dbContext));
        _disciplines = new Lazy<IDisciplineRepository>(() => new DisciplineRepository(_dbContext));
    }

    public ICompanyRepository Companies => _companies.Value;
    public IFormatRepository Formats => _formats.Value;
    public IDisciplineRepository Disciplines => _disciplines.Value;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = _dbContext.ChangeTracker.Entries<IAggregateRoot>()
            .SelectMany(entry => entry.Entity.DomainEvents)
            .ToList();

        var now = DateTime.UtcNow;
        var auditLogs = domainEvents
            .Select(domainEvent => _auditedHandler.CreateAuditLog(domainEvent, _currentUser.UserId, now))
            .Where(log => log is not null)
            .Select(log => log!)
            .ToList();

        var result = await _dbContext.SaveChangesAsync(cancellationToken);

        if (auditLogs.Count > 0)
        {
            _dbContext.CatalogChanges.AddRange(auditLogs);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        foreach (var entry in _dbContext.ChangeTracker.Entries<IAggregateRoot>())
        {
            entry.Entity.ClearDomainEvents();
        }

        return result;
    }

    public ValueTask DisposeAsync() => _dbContext.DisposeAsync();
}
