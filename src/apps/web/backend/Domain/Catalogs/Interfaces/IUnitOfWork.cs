using Domain.Catalogs.Interfaces;

namespace Domain.Catalogs.Interfaces;

public interface IUnitOfWork : IAsyncDisposable
{
    ICompanyRepository Companies { get; }
    IFormatRepository Formats { get; }
    IDisciplineRepository Disciplines { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
