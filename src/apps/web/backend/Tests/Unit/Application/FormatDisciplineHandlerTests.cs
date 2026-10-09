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
public class FormatDisciplineHandlerTests
{
    private sealed class InMemoryFormatRepository : IFormatRepository
    {
        private readonly Dictionary<Guid, Format> _items = new();
        private readonly HashSet<Guid> _usage = new();

        public Task<Format?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_items.TryGetValue(id, out var item) ? item : null);

        public Task<Format?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
            Task.FromResult(_items.Values.FirstOrDefault(x =>
                string.Equals(x.Code.Value, code, StringComparison.OrdinalIgnoreCase)));

        public Task<Format?> GetByDescriptionAsync(string description, CancellationToken cancellationToken = default) =>
            Task.FromResult(_items.Values.FirstOrDefault(x =>
                string.Equals(x.Description.Value, description, StringComparison.OrdinalIgnoreCase)));

        public Task<IReadOnlyList<Format>> GetPagedAsync(int skip, int take, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Format>>(_items.Values.Skip(skip).Take(take).ToList());

        public Task<int> CountAsync(CancellationToken cancellationToken = default) => Task.FromResult(_items.Count);

        public Task<IReadOnlyList<Format>> SearchAsync(
            string? code, string? description, int skip, int take, CancellationToken cancellationToken = default)
        {
            IEnumerable<Format> query = _items.Values;
            if (!string.IsNullOrWhiteSpace(code))
            {
                query = query.Where(x => x.Code.Value.Contains(code, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrWhiteSpace(description))
            {
                query = query.Where(x => x.Description.Value.Contains(description, StringComparison.OrdinalIgnoreCase));
            }

            return Task.FromResult<IReadOnlyList<Format>>(query.Skip(skip).Take(take).ToList());
        }

        public Task<int> CountSearchAsync(string? code, string? description, CancellationToken cancellationToken = default) =>
            Task.FromResult(SearchAsync(code, description, 0, int.MaxValue, cancellationToken).Result.Count);

        public Task<int> GetUsageCountAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_usage.Contains(id) ? 1 : 0);

        public Task AddAsync(Format format, CancellationToken cancellationToken = default)
        {
            _items[format.Id] = format;
            return Task.CompletedTask;
        }

        public void Update(Format format) => _items[format.Id] = format;
        public void Remove(Format format) => _items.Remove(format.Id);
        public void MarkInUse(Guid id) => _usage.Add(id);
    }

    private sealed class InMemoryDisciplineRepository : IDisciplineRepository
    {
        private readonly Dictionary<Guid, Discipline> _items = new();
        private readonly HashSet<Guid> _usage = new();

        public Task<Discipline?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_items.TryGetValue(id, out var item) ? item : null);

        public Task<Discipline?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
            Task.FromResult(_items.Values.FirstOrDefault(x =>
                string.Equals(x.Code.Value, code, StringComparison.OrdinalIgnoreCase)));

        public Task<Discipline?> GetByDescriptionAsync(string description, CancellationToken cancellationToken = default) =>
            Task.FromResult(_items.Values.FirstOrDefault(x =>
                string.Equals(x.Description.Value, description, StringComparison.OrdinalIgnoreCase)));

        public Task<IReadOnlyList<Discipline>> GetPagedAsync(int skip, int take, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Discipline>>(_items.Values.Skip(skip).Take(take).ToList());

        public Task<int> CountAsync(CancellationToken cancellationToken = default) => Task.FromResult(_items.Count);

        public Task<IReadOnlyList<Discipline>> SearchAsync(
            string? code, string? description, int skip, int take, CancellationToken cancellationToken = default)
        {
            IEnumerable<Discipline> query = _items.Values;
            if (!string.IsNullOrWhiteSpace(code))
            {
                query = query.Where(x => x.Code.Value.Contains(code, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrWhiteSpace(description))
            {
                query = query.Where(x => x.Description.Value.Contains(description, StringComparison.OrdinalIgnoreCase));
            }

            return Task.FromResult<IReadOnlyList<Discipline>>(query.Skip(skip).Take(take).ToList());
        }

        public Task<int> CountSearchAsync(string? code, string? description, CancellationToken cancellationToken = default) =>
            Task.FromResult(SearchAsync(code, description, 0, int.MaxValue, cancellationToken).Result.Count);

        public Task<int> GetUsageCountAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_usage.Contains(id) ? 1 : 0);

        public Task AddAsync(Discipline discipline, CancellationToken cancellationToken = default)
        {
            _items[discipline.Id] = discipline;
            return Task.CompletedTask;
        }

        public void Update(Discipline discipline) => _items[discipline.Id] = discipline;
        public void Remove(Discipline discipline) => _items.Remove(discipline.Id);
        public void MarkInUse(Guid id) => _usage.Add(id);
    }

    private sealed class FormatUnitOfWork : IUnitOfWork
    {
        public InMemoryFormatRepository Formats { get; } = new();
        ICompanyRepository IUnitOfWork.Companies => throw new NotSupportedException();
        IFormatRepository IUnitOfWork.Formats => Formats;
        IDisciplineRepository IUnitOfWork.Disciplines => throw new NotSupportedException();
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    private sealed class DisciplineUnitOfWork : IUnitOfWork
    {
        public InMemoryDisciplineRepository Disciplines { get; } = new();
        ICompanyRepository IUnitOfWork.Companies => throw new NotSupportedException();
        IFormatRepository IUnitOfWork.Formats => throw new NotSupportedException();
        IDisciplineRepository IUnitOfWork.Disciplines => Disciplines;
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    [Test]
    public async Task Format_CreateAndSearch()
    {
        var uow = new FormatUnitOfWork();
        var create = new CreateFormatCommandHandler(uow);
        await create.Handle(new CreateFormatCommand("FMT001", "Video"), CancellationToken.None);
        await create.Handle(new CreateFormatCommand("FMT002", "Impreso"), CancellationToken.None);

        var search = new SearchFormatsQueryHandler(uow);
        var result = await search.Handle(new SearchFormatsQuery("fmt002", null, 0, 10), CancellationToken.None);

        Assert.That(result.Total, Is.EqualTo(1));
        Assert.That(result.Items.Single().Code, Is.EqualTo("FMT002"));
    }

    [Test]
    public async Task Format_DuplicateCode_Throws()
    {
        var uow = new FormatUnitOfWork();
        var create = new CreateFormatCommandHandler(uow);
        await create.Handle(new CreateFormatCommand("FMT001", "Video"), CancellationToken.None);

        Assert.ThrowsAsync<DuplicateCodeException>(
            () => create.Handle(new CreateFormatCommand("fmt001", "Otro"), CancellationToken.None));
    }

    [Test]
    public async Task Format_UpdateInUse_OnlyDescriptionChanges()
    {
        var uow = new FormatUnitOfWork();
        var create = new CreateFormatCommandHandler(uow);
        var id = await create.Handle(new CreateFormatCommand("FMT001", "Video"), CancellationToken.None);
        uow.Formats.MarkInUse(id);
        var update = new UpdateFormatCommandHandler(uow);

        await update.Handle(new UpdateFormatCommand(id, "CHANGED", "Video actualizado"), CancellationToken.None);

        var format = await uow.Formats.GetByIdAsync(id);
        Assert.That(format!.Code.Value, Is.EqualTo("FMT001"));
        Assert.That(format.Description.Value, Is.EqualTo("Video actualizado"));
    }

    [Test]
    public async Task Format_DeleteInUse_Throws()
    {
        var uow = new FormatUnitOfWork();
        var create = new CreateFormatCommandHandler(uow);
        var id = await create.Handle(new CreateFormatCommand("FMT001", "Video"), CancellationToken.None);
        uow.Formats.MarkInUse(id);
        var delete = new DeleteFormatCommandHandler(uow);

        Assert.ThrowsAsync<CatalogInUseException>(
            () => delete.Handle(new DeleteFormatCommand(id), CancellationToken.None));
    }

    [Test]
    public async Task Discipline_CreateUpdateAndList()
    {
        var uow = new DisciplineUnitOfWork();
        var create = new CreateDisciplineCommandHandler(uow);
        var id = await create.Handle(new CreateDisciplineCommand("DSC001", "Geología"), CancellationToken.None);
        var update = new UpdateDisciplineCommandHandler(uow);
        await update.Handle(new UpdateDisciplineCommand(id, "DSC002", "Metalurgia"), CancellationToken.None);

        var list = new GetDisciplinesQueryHandler(uow);
        var result = await list.Handle(new GetDisciplinesQuery(0, 10), CancellationToken.None);

        Assert.That(result.Total, Is.EqualTo(1));
        Assert.That(result.Items.Single().Code, Is.EqualTo("DSC002"));
    }

    [Test]
    public async Task Discipline_DuplicateDescription_Throws()
    {
        var uow = new DisciplineUnitOfWork();
        var create = new CreateDisciplineCommandHandler(uow);
        await create.Handle(new CreateDisciplineCommand("DSC001", "Geología"), CancellationToken.None);

        Assert.ThrowsAsync<DuplicateDescriptionException>(
            () => create.Handle(new CreateDisciplineCommand("DSC002", "geología"), CancellationToken.None));
    }
}
