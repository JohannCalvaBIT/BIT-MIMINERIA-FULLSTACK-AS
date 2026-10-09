using Domain.Catalogs.Events;
using Application.Catalogs.Commands;
using Application.Catalogs.DTOs;
using Application.Catalogs.Queries;
using Domain.Catalogs.Entities;
using Domain.Catalogs.Exceptions;
using Domain.Catalogs.Interfaces;
using Domain.Catalogs.ValueObjects;
using NUnit.Framework;

namespace Tests.Unit.Application;

[TestFixture]
public class CatalogHandlerTests
{
    private sealed class InMemoryCompanyRepository : ICompanyRepository
    {
        private readonly Dictionary<Guid, Company> _items = new();
        private readonly List<CompanyCreatedEvent> _published = new();

        public IReadOnlyList<CompanyCreatedEvent> Published => _published;

        public Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_items.TryGetValue(id, out var item) ? item : null);

        public Task<Company?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
            Task.FromResult(_items.Values.FirstOrDefault(x =>
                string.Equals(x.Code.Value, code, StringComparison.OrdinalIgnoreCase)));

        public Task<Company?> GetByDescriptionAsync(string description, CancellationToken cancellationToken = default) =>
            Task.FromResult(_items.Values.FirstOrDefault(x =>
                string.Equals(x.Description.Value, description, StringComparison.OrdinalIgnoreCase)));

        public Task<IReadOnlyList<Company>> GetPagedAsync(int skip, int take, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Company>>(_items.Values.Skip(skip).Take(take).ToList());

        public Task<int> CountAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(_items.Count);

        public Task<IReadOnlyList<Company>> SearchAsync(string? code, string? description, int skip, int take, CancellationToken cancellationToken = default)
        {
            IEnumerable<Company> query = _items.Values;
            if (!string.IsNullOrWhiteSpace(code))
            {
                query = query.Where(x => x.Code.Value.Contains(code, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrWhiteSpace(description))
            {
                query = query.Where(x => x.Description.Value.Contains(description, StringComparison.OrdinalIgnoreCase));
            }
            return Task.FromResult<IReadOnlyList<Company>>(query.Skip(skip).Take(take).ToList());
        }

        public Task<int> CountSearchAsync(string? code, string? description, CancellationToken cancellationToken = default)
        {
            IEnumerable<Company> query = _items.Values;
            if (!string.IsNullOrWhiteSpace(code))
            {
                query = query.Where(x => x.Code.Value.Contains(code, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrWhiteSpace(description))
            {
                query = query.Where(x => x.Description.Value.Contains(description, StringComparison.OrdinalIgnoreCase));
            }

            return Task.FromResult(query.Count());
        }

        public Task<int> GetUsageCountAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_usage.Contains(id) ? 1 : 0);

        private readonly HashSet<Guid> _usage = new();

        public void MarkInUse(Guid id) => _usage.Add(id);

        public Task AddAsync(Company company, CancellationToken cancellationToken = default)
        {
            _items[company.Id] = company;
            return Task.CompletedTask;
        }

        public void Update(Company company) => _items[company.Id] = company;

        public void Remove(Company company) => _items.Remove(company.Id);
    }

    private sealed class InMemoryUnitOfWork : IUnitOfWork
    {
        public InMemoryCompanyRepository Companies { get; } = new();
        ICompanyRepository IUnitOfWork.Companies => Companies;
        public IFormatRepository Formats => throw new NotSupportedException();
        public IDisciplineRepository Disciplines => throw new NotSupportedException();
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    private InMemoryUnitOfWork _uow = null!;
    private CreateCompanyCommandHandler _create = null!;

    [SetUp]
    public void SetUp()
    {
        _uow = new InMemoryUnitOfWork();
        _create = new CreateCompanyCommandHandler(_uow);
    }

    [Test]
    public async Task Create_HappyPath_ReturnsIdAndPersists()
    {
        var id = await _create.Handle(new CreateCompanyCommand("EMP001", "Mi Minería S.A."), CancellationToken.None);
        Assert.That(id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(_uow.Companies.CountAsync().Result, Is.EqualTo(1));
    }

    [Test]
    public async Task Create_DuplicateCode_Throws()
    {
        await _create.Handle(new CreateCompanyCommand("EMP001", "Mi Minería S.A."), CancellationToken.None);
        Assert.ThrowsAsync<DuplicateCodeException>(
            () => _create.Handle(new CreateCompanyCommand("emp001", "Otra S.A."), CancellationToken.None));
    }

    [Test]
    public async Task Create_DuplicateDescription_Throws()
    {
        await _create.Handle(new CreateCompanyCommand("EMP001", "Mi Minería S.A."), CancellationToken.None);
        Assert.ThrowsAsync<DuplicateDescriptionException>(
            () => _create.Handle(new CreateCompanyCommand("EMP002", "mi minería s.a."), CancellationToken.None));
    }

    [Test]
    public void Create_EmptyCode_Throws()
    {
        Assert.ThrowsAsync<ArgumentException>(
            () => _create.Handle(new CreateCompanyCommand("", "Mi Minería S.A."), CancellationToken.None));
    }

    [Test]
    public void Create_TooLongCode_Throws()
    {
        Assert.ThrowsAsync<ArgumentException>(
            () => _create.Handle(new CreateCompanyCommand(new string('a', 151), "Mi Minería S.A."), CancellationToken.None));
    }

    [Test]
    public async Task Update_WhenInUse_AllowsDescriptionOnly()
    {
        var id = await _create.Handle(new CreateCompanyCommand("EMP001", "Mi Minería S.A."), CancellationToken.None);
        _uow.Companies.MarkInUse(id);
        var handler = new UpdateCompanyCommandHandler(_uow);

        await handler.Handle(new UpdateCompanyCommand(id, "CHANGED", "Nueva Descripción"), CancellationToken.None);

        var company = await _uow.Companies.GetByIdAsync(id);
        Assert.That(company!.Code.Value, Is.EqualTo("EMP001"));
        Assert.That(company.Description.Value, Is.EqualTo("Nueva Descripción"));
    }

    [Test]
    public async Task Update_WhenNotInUse_AllowsCodeChange()
    {
        var id = await _create.Handle(new CreateCompanyCommand("EMP001", "Mi Minería S.A."), CancellationToken.None);
        var handler = new UpdateCompanyCommandHandler(_uow);

        await handler.Handle(new UpdateCompanyCommand(id, "EMP002", "Mi Minería S.A."), CancellationToken.None);

        var company = await _uow.Companies.GetByIdAsync(id);
        Assert.That(company!.Code.Value, Is.EqualTo("EMP002"));
    }

    [Test]
    public async Task Update_WhenNotInUse_DuplicateCode_Throws()
    {
        var id = await _create.Handle(new CreateCompanyCommand("EMP001", "Mi Minería S.A."), CancellationToken.None);
        await _create.Handle(new CreateCompanyCommand("EMP002", "Otra empresa"), CancellationToken.None);
        var handler = new UpdateCompanyCommandHandler(_uow);

        Assert.ThrowsAsync<DuplicateCodeException>(
            () => handler.Handle(new UpdateCompanyCommand(id, "EMP002", "Mi Minería S.A."), CancellationToken.None));
    }

    [Test]
    public async Task Update_DuplicateDescription_Throws()
    {
        var id = await _create.Handle(new CreateCompanyCommand("EMP001", "Mi Minería S.A."), CancellationToken.None);
        await _create.Handle(new CreateCompanyCommand("EMP002", "Otra empresa"), CancellationToken.None);
        var handler = new UpdateCompanyCommandHandler(_uow);

        Assert.ThrowsAsync<DuplicateDescriptionException>(
            () => handler.Handle(new UpdateCompanyCommand(id, "EMP001", "otra empresa"), CancellationToken.None));
    }

    [Test]
    public async Task Delete_WhenInUse_Throws()
    {
        var id = await _create.Handle(new CreateCompanyCommand("EMP001", "Mi Minería S.A."), CancellationToken.None);
        _uow.Companies.MarkInUse(id);
        var handler = new DeleteCompanyCommandHandler(_uow);

        Assert.ThrowsAsync<CatalogInUseException>(
            () => handler.Handle(new DeleteCompanyCommand(id), CancellationToken.None));
    }

    [Test]
    public async Task Delete_WhenNotInUse_Removes()
    {
        var id = await _create.Handle(new CreateCompanyCommand("EMP001", "Mi Minería S.A."), CancellationToken.None);
        var handler = new DeleteCompanyCommandHandler(_uow);

        await handler.Handle(new DeleteCompanyCommand(id), CancellationToken.None);

        Assert.That(await _uow.Companies.GetByIdAsync(id), Is.Null);
    }

    [Test]
    public async Task GetCompanies_ReturnsPagedItemsAndUsageFlag()
    {
        var firstId = await _create.Handle(new CreateCompanyCommand("EMP001", "Mi Minería S.A."), CancellationToken.None);
        await _create.Handle(new CreateCompanyCommand("EMP002", "Otra empresa"), CancellationToken.None);
        _uow.Companies.MarkInUse(firstId);
        var handler = new GetCompaniesQueryHandler(_uow);

        var result = await handler.Handle(new GetCompaniesQuery(0, 10), CancellationToken.None);

        Assert.That(result.Total, Is.EqualTo(2));
        Assert.That(result.Items, Has.Count.EqualTo(2));
        Assert.That(result.Items.Single(x => x.Id == firstId).IsInUse, Is.True);
    }

    [Test]
    public async Task SearchCompanies_FiltersByCodeAndDescription()
    {
        await _create.Handle(new CreateCompanyCommand("EMP001", "Mi Minería S.A."), CancellationToken.None);
        await _create.Handle(new CreateCompanyCommand("EMP002", "Otra empresa"), CancellationToken.None);
        var handler = new SearchCompaniesQueryHandler(_uow);

        var byCode = await handler.Handle(new SearchCompaniesQuery("emp002", null, 0, 10), CancellationToken.None);
        var byDescription = await handler.Handle(new SearchCompaniesQuery(null, "minería", 0, 10), CancellationToken.None);

        Assert.That(byCode.Items.Single().Code, Is.EqualTo("EMP002"));
        Assert.That(byDescription.Items.Single().Code, Is.EqualTo("EMP001"));
    }
}
