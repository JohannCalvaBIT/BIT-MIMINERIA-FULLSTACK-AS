using Domain.Catalogs.Entities;
using Domain.Catalogs.Interfaces;
using Infrastructure.Catalogs.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Catalogs.Repositories;

public sealed class FormatRepository : IFormatRepository
{
    private readonly CatalogDbContext _dbContext;

    public FormatRepository(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Format?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Formats.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Format?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        _dbContext.Formats.AsNoTracking().FirstOrDefaultAsync(
            x => EF.Functions.Collate(x.Code.Value, "Latin1_General_CI_AS") == code,
            cancellationToken);

    public Task<Format?> GetByDescriptionAsync(string description, CancellationToken cancellationToken = default) =>
        _dbContext.Formats.AsNoTracking().FirstOrDefaultAsync(
            x => EF.Functions.Collate(x.Description.Value, "Latin1_General_CI_AS") == description,
            cancellationToken);

    public async Task<IReadOnlyList<Format>> GetPagedAsync(int skip, int take, CancellationToken cancellationToken = default) =>
        await _dbContext.Formats.AsNoTracking()
            .OrderBy(x => x.Code.Value)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(CancellationToken cancellationToken = default) =>
        _dbContext.Formats.CountAsync(cancellationToken);

    public async Task<IReadOnlyList<Format>> SearchAsync(
        string? code, string? description, int skip, int take, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Formats.AsNoTracking().AsQueryable();

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
        var query = _dbContext.Formats.AsNoTracking().AsQueryable();

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

    public Task AddAsync(Format format, CancellationToken cancellationToken = default)
    {
        _dbContext.Formats.Add(format);
        return Task.CompletedTask;
    }

    public void Update(Format format)
    {
        _dbContext.Formats.Update(format);
    }

    public void Remove(Format format)
    {
        _dbContext.Formats.Remove(format);
    }
}
