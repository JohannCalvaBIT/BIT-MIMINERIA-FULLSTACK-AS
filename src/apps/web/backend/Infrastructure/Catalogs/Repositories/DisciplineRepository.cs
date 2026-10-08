using Domain.Catalogs.Entities;
using Domain.Catalogs.Interfaces;
using Infrastructure.Catalogs.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Catalogs.Repositories;

public sealed class DisciplineRepository : IDisciplineRepository
{
    private readonly CatalogDbContext _dbContext;

    public DisciplineRepository(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Discipline?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Disciplines.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Discipline?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        _dbContext.Disciplines.AsNoTracking().FirstOrDefaultAsync(
            x => EF.Functions.Collate(x.Code.Value, "Latin1_General_CI_AS") == code,
            cancellationToken);

    public Task<Discipline?> GetByDescriptionAsync(string description, CancellationToken cancellationToken = default) =>
        _dbContext.Disciplines.AsNoTracking().FirstOrDefaultAsync(
            x => EF.Functions.Collate(x.Description.Value, "Latin1_General_CI_AS") == description,
            cancellationToken);

    public async Task<IReadOnlyList<Discipline>> GetPagedAsync(int skip, int take, CancellationToken cancellationToken = default) =>
        await _dbContext.Disciplines.AsNoTracking()
            .OrderBy(x => x.Code.Value)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(CancellationToken cancellationToken = default) =>
        _dbContext.Disciplines.CountAsync(cancellationToken);

    public async Task<IReadOnlyList<Discipline>> SearchAsync(
        string? code, string? description, int skip, int take, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Disciplines.AsNoTracking().AsQueryable();

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
        var query = _dbContext.Disciplines.AsNoTracking().AsQueryable();

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

    public Task AddAsync(Discipline discipline, CancellationToken cancellationToken = default)
    {
        _dbContext.Disciplines.Add(discipline);
        return Task.CompletedTask;
    }

    public void Update(Discipline discipline)
    {
        _dbContext.Disciplines.Update(discipline);
    }

    public void Remove(Discipline discipline)
    {
        _dbContext.Disciplines.Remove(discipline);
    }
}
