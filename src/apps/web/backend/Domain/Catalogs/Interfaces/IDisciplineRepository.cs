using Domain.Catalogs.Entities;

namespace Domain.Catalogs.Interfaces;

public interface IDisciplineRepository
{
    Task<Discipline?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Discipline?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<Discipline?> GetByDescriptionAsync(string description, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Discipline>> GetPagedAsync(int skip, int take, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Discipline>> SearchAsync(string? code, string? description, int skip, int take, CancellationToken cancellationToken = default);
    Task<int> CountSearchAsync(string? code, string? description, CancellationToken cancellationToken = default);
    Task<int> GetUsageCountAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Discipline discipline, CancellationToken cancellationToken = default);
    void Update(Discipline discipline);
    void Remove(Discipline discipline);
}
