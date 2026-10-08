using Domain.Catalogs.Entities;

namespace Domain.Catalogs.Interfaces;

public interface IFormatRepository
{
    Task<Format?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Format?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<Format?> GetByDescriptionAsync(string description, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Format>> GetPagedAsync(int skip, int take, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Format>> SearchAsync(string? code, string? description, int skip, int take, CancellationToken cancellationToken = default);
    Task<int> CountSearchAsync(string? code, string? description, CancellationToken cancellationToken = default);
    Task<int> GetUsageCountAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Format format, CancellationToken cancellationToken = default);
    void Update(Format format);
    void Remove(Format format);
}
