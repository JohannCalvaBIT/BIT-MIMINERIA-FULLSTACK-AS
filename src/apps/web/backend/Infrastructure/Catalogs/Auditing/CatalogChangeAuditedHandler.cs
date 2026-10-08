using Domain.Catalogs.Events;
using Infrastructure.Catalogs.Persistence;
using System.Text.Json;

namespace Infrastructure.Catalogs.Auditing;

public sealed class CatalogChangeAuditedHandler
{
    private sealed record AuditMeta(
        string EntityType,
        Guid EntityId,
        string Action,
        string? OldValue,
        string? NewValue);

    public CatalogAuditLog? CreateAuditLog(IDomainEvent domainEvent, string? userId, DateTime now)
    {
        var meta = domainEvent switch
        {
            CompanyCreatedEvent e => new AuditMeta("Company", e.EntityId, "CREATE", null, Json(e.Code, e.Description)),
            CompanyUpdatedEvent e => new AuditMeta("Company", e.EntityId, "UPDATE", null, Json(e.Code, e.Description)),
            CompanyDeletedEvent e => new AuditMeta("Company", e.EntityId, "DELETE", Json(e.Code, null), null),
            FormatCreatedEvent e => new AuditMeta("Format", e.EntityId, "CREATE", null, Json(e.Code, e.Description)),
            FormatUpdatedEvent e => new AuditMeta("Format", e.EntityId, "UPDATE", null, Json(e.Code, e.Description)),
            FormatDeletedEvent e => new AuditMeta("Format", e.EntityId, "DELETE", Json(e.Code, null), null),
            DisciplineCreatedEvent e => new AuditMeta("Discipline", e.EntityId, "CREATE", null, Json(e.Code, e.Description)),
            DisciplineUpdatedEvent e => new AuditMeta("Discipline", e.EntityId, "UPDATE", null, Json(e.Code, e.Description)),
            DisciplineDeletedEvent e => new AuditMeta("Discipline", e.EntityId, "DELETE", Json(e.Code, null), null),
            _ => new AuditMeta("Unknown", Guid.Empty, "UNKNOWN", null, null)
        };

        if (meta.EntityType == "Unknown")
        {
            return null;
        }

        return new CatalogAuditLog
        {
            Id = Guid.NewGuid(),
            EntityType = meta.EntityType,
            EntityId = meta.EntityId,
            Action = meta.Action,
            UserId = userId,
            Timestamp = now,
            OldValue = meta.OldValue,
            NewValue = meta.NewValue
        };
    }

    private static string Json(string code, string? description)
    {
        var payload = new Dictionary<string, string> { ["code"] = code };
        if (description is not null)
        {
            payload["description"] = description;
        }

        return JsonSerializer.Serialize(payload);
    }
}
