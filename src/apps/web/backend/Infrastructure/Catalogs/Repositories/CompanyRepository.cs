using Domain.Catalogs.Entities;
using Domain.Catalogs.Interfaces;
using Infrastructure.Catalogs.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Catalogs.Repositories;

public sealed class CompanyRepository : ICompanyRepository
{
    private readonly CatalogDbContext _dbContext;

    public CompanyRepository(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Companies.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Company?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        _dbContext.Companies.AsNoTracking().FirstOrDefaultAsync(
            x => EF.Functions.Collate(x.Code.Value, "Latin1_General_CI_AS") == code,
            cancellationToken);

    public Task<Company?> GetByDescriptionAsync(string description, CancellationToken cancellationToken = default) =>
        _dbContext.Companies.AsNoTracking().FirstOrDefaultAsync(
            x => EF.Functions.Collate(x.Description.Value, "Latin1_General_CI_AS") == description,
            cancellationToken);

    public async Task<IReadOnlyList<Company>> GetPagedAsync(int skip, int take, CancellationToken cancellationToken = default) =>
        await _dbContext.Companies.AsNoTracking()
            .OrderBy(x => x.Code.Value)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(CancellationToken cancellationToken = default) =>
        _dbContext.Companies.CountAsync(cancellationToken);

    public async Task<IReadOnlyList<Company>> SearchAsync(
        string? code, string? description, int skip, int take, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Companies.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(code))
        {
            query = query.Where(x => EF.Functions.Like(x.Code.Value, $"%{code}%"));
        }

        if (!string.IsNullOrWhiteSpace(description))
        {
            query = query.Where(x => EF.Functions.Like(x.Description.Value, $"%{description}%"));
        }

        return await query.OrderBy(x => x.Code.Value).Skip(skip).Take(take).ToListAsync(cancellationToken);
    }

    public Task<int> CountSearchAsync(
        string? code, string? description, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Companies.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(code))
        {
            query = query.Where(x => EF.Functions.Like(x.Code.Value, $"%{code}%"));
        }

        if (!string.IsNullOrWhiteSpace(description))
        {
            query = query.Where(x => EF.Functions.Like(x.Description.Value, $"%{description}%"));
        }

        return query.CountAsync(cancellationToken);
    }

    public Task<int> GetUsageCountAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(0);

    public Task AddAsync(Company company, CancellationToken cancellationToken = default)
    {
        _dbContext.Companies.Add(company);
        return Task.CompletedTask;
    }

    public void Update(Company company)
    {
        _dbContext.Companies.Update(company);
    }

    public void Remove(Company company)
    {
        _dbContext.Companies.Remove(company);
    }
}
