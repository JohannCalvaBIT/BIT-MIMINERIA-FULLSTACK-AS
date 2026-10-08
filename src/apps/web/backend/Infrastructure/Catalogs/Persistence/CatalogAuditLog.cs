using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Catalogs.Persistence;

[PrimaryKey(nameof(Id))]
public sealed class CatalogAuditLog
{
    public Guid Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public DateTime Timestamp { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}
