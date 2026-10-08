using Domain.Catalogs.Entities;

namespace Domain.Catalogs.Interfaces;

public interface IDisciplineRepository
{
    Task<Discipline?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Discipline?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<Discipline?> GetByDescriptionAsync(string description, CancellationToken cancellationToken = default);
    Task<int> GetUsageCountAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Discipline discipline, CancellationToken cancellationToken = default);
    void Update(Discipline discipline);
    void Remove(Discipline discipline);
}
