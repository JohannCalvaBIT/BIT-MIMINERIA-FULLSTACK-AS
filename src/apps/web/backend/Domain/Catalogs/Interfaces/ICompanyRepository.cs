using Domain.Catalogs.Entities;

namespace Domain.Catalogs.Interfaces;

public interface ICompanyRepository
{
    Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Company?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<Company?> GetByDescriptionAsync(string description, CancellationToken cancellationToken = default);
    Task<int> GetUsageCountAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Company company, CancellationToken cancellationToken = default);
    void Update(Company company);
    void Remove(Company company);
}
