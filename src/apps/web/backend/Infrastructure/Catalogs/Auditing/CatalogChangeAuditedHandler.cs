using Domain.Catalogs.Events;
using Infrastructure.Catalogs.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Infrastructure.Catalogs.Auditing;

public sealed class CatalogChangeAuditedHandler : IHostedLifecycleService
{
    private readonly ILogger<CatalogChangeAuditedHandler> _logger;

    public CatalogChangeAuditedHandler(ILogger<CatalogChangeAuditedHandler> logger)
    {
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }

    public async Task StartedAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }

    public async Task StartingAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }

    public async Task StoppedAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }

    public async Task StoppingAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
}
