using Domain.Catalogs.Entities;

namespace Domain.Catalogs.Interfaces;

public interface IFormatRepository
{
    Task<Format?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Format?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<Format?> GetByDescriptionAsync(string description, CancellationToken cancellationToken = default);
    Task<int> GetUsageCountAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Format format, CancellationToken cancellationToken = default);
    void Update(Format format);
    void Remove(Format format);
}
