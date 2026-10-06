# GUARDRAILS - Backend Developer (.NET 10)

**Rol**: Backend Developer  
**Stack**: .NET 10.0 + C# 14 + ASP.NET Core 10 + Entity Framework Core 10  
**Última actualización**: 2026-09-23

---

## G-WEB-BE-01: Hexagonal Architecture Rules

### Layer Boundaries (NO Cross-Layer Imports)

```
✅ CORRECT:
Presentation → Application → Domain ← Infrastructure (via interface)
          ↓        ↓
         Core    Core

❌ WRONG:
- Presentation directly uses Infrastructure
- Infrastructure imports Application
- Domain imports from Infrastructure
- Circular dependencies
```

### File Organization
```
Solution/
├── Domain/                    # Business logic
│   ├── Entities/
│   ├── Interfaces/            # IOrderRepository, IPaymentGateway
│   └── Exceptions/
│
├── Application/               # Orchestration
│   ├── Commands/
│   ├── Queries/
│   ├── DTOs/
│   └── Services/
│
├── Infrastructure/            # Concrete Implementations
│   ├── Repositories/          # Implements IOrderRepository
│   ├── DataSources/           # API clients, DB queries
│   └── Mappers/               # Entity ↔ Model ↔ DTO
│
├── Presentation/              # Web API
│   └── Controllers/
│
└── Tests/
```

---

## G-WEB-BE-02: Database Rules

### Queries
- ✅ Parameterized ALWAYS: `@parameter` in SQL
- ✅ `Include()` for eager loading (prevent N+1)
- ✅ `AsNoTracking()` for read-only queries (performance)
- ✅ Paging for large datasets: `Skip(page * size).Take(size)`
- ❌ String concatenation for queries (SQL injection risk)
- ❌ SELECT * (specify columns)
- ❌ Unfiltered queries without pagination

### Schema Design
- ✅ Primary keys: UUID or GUID (multi-tenant, sync)
- ✅ Timestamps: `CreatedAt`, `UpdatedAt` on all tables
- ✅ Soft delete: `DeletedAt` nullable (for sync)
- ✅ Indices: on foreign keys, search fields, filters
- ✅ Constraints: check constraints for domain rules
- ❌ Composite keys (prefer UUID + business key)
- ❌ Vague column names: `data`, `info`, `temp`

### Migrations
**Autoridad: `guardrails-10-database-modeling.md` (G-DB-02/G-DB-03) — Script-Only, sin migraciones de EF Core.** EF Core en este proyecto es el ORM para *consultar* (DbContext, LINQ, `AsNoTracking()`), no el mecanismo de evolución de esquema — el esquema se versiona con scripts SQL planos (`database/000_Baseline/`, `001_Initial/`, etc.), no con `dotnet ef migrations add`.
- ✅ Todo cambio de esquema es un script SQL versionado en `database/`, no una migración de EF Core
- ✅ Reversible: cada script tiene su `_Rollback.sql` (ver G-DB-02)
- ✅ Probado: los scripts se validan en CI/CD antes de testing (ver G-DB-03)
- ❌ `dotnet ef migrations add` / clases `*.Migrations.*` generadas por EF Core
- ❌ `auto-create`/`EnsureCreated()` en producción (el esquema lo controla `database/`, no EF Core)

---

## G-WEB-BE-03: API Endpoint Rules

### HTTP Methods Correctness
- ✅ `GET`: retrieve data (idempotent, cacheable, safe)
- ✅ `POST`: create new resource (idempotent key recommended)
- ✅ `PUT`: replace entire resource (idempotent, complete)
- ✅ `PATCH`: partial update (idempotent, partial)
- ✅ `DELETE`: remove resource (idempotent)
- ❌ GET for create/update/delete (state-changing side effects)
- ❌ POST for retrieval (filters/search should use GET + query params)

### Status Codes
```
200 OK              : GET success
201 Created         : POST success (include Location header)
204 No Content      : DELETE success (no body)
400 Bad Request     : Validation failed (client error)
401 Unauthorized    : Auth required/failed
403 Forbidden       : Auth OK but not permitted
404 Not Found       : Resource doesn't exist
409 Conflict        : Resource state conflict
500 Internal Server : Unhandled exception (log, alert)
```

### Response Consistency
- ✅ Always return standard format:
  ```json
  { "success": true, "data": {...}, "message": "...", "meta": {...} }
  ```
- ✅ Error format:
  ```json
  { "success": false, "error": { "code": "ERROR_CODE", "message": "..." } }
  ```
- ❌ Inconsistent formats (breaks client parsing)
- ❌ Error info leaking (stack traces in production)

---

## G-WEB-BE-04: Authentication & Authorization

Cuando la identidad está delegada a un IdP externo (Microsoft Entra ID u otro — el Infrastructure Lock del framework fija Entra ID), el backend **NUNCA** emite, firma ni renueva tokens de usuario propios — el IdP es el único emisor.

### Delegación al IdP (Authorization Code + PKCE)
- ✅ El cliente (SPA/app) obtiene el token directamente del IdP (Authorization Code + PKCE)
- ✅ El API gateway valida firma, expiración y audience en cada request, antes de reenviar al backend
- ✅ El backend vuelve a validar el token (defensa en profundidad) vía el middleware estándar de JWT Bearer, configurado contra el metadata OIDC del IdP
- ✅ Almacenamiento del token en el cliente: `sessionStorage` (vía MSAL.js u otro SDK del IdP) — o `httpOnly` cookies si el flujo usa un patrón BFF (Backend-for-Frontend)
- ❌ Emitir JWT propios para usuarios (`/api/*/auth/*` de login/refresh)
- ❌ Persistir refresh tokens de usuario en la base de datos de la aplicación
- ❌ Credenciales en query string (usar `Authorization: Bearer`)
- ❌ Que el cliente guarde tokens en `localStorage`

### Authorization Checks
- ✅ Autorización: rol/permiso se resuelve server-side contra datos propios de la plataforma — nunca confiando en claims (roles/groups) leídas directamente del token por el cliente
- ✅ Every endpoint validates:
  1. Token validity (expired, signature, audience, issuer)
  2. User exists & active
  3. User has required role/permission (resuelto server-side)
- ✅ Multi-tenant (solo si el proyecto es multi-tenant, ver G-DB-01/G-DB-06 en guardrails-10-database-modeling.md): validate `businessId` from token matches resource
- ❌ Skipping any validation step
- ❌ Assuming token is valid (always verify)

---

## G-WEB-BE-05: Error Handling

### Exception Strategy
- ✅ Custom exceptions: `OrderNotFoundException`, `InvalidPaymentException`
- ✅ Global exception handler middleware
- ✅ Logging structured with correlation ID
- ✅ Client sees sanitized error (no stack traces)
- ✅ Internal errors logged with full context
- ❌ Generic "Server Error" (client can't debug)
- ❌ Exception details in response (leaks internals)

### Logging
```csharp
// ✅ CORRECT
_logger.LogError(ex, "Order creation failed for business {BusinessId}", businessId);

// ❌ WRONG
_logger.LogError("Error: " + ex.Message);
_logger.LogError(ex, "Critical: {FullStackTrace}", ex);  // Too verbose
```

---

## G-WEB-BE-06: Domain Events

### Domain Event Definition
- ✅ All significant domain state changes raise domain events (entity created, status changed, business rule triggered)
- ✅ Events defined in Domain layer: plain objects with meaningful name (e.g., `OrderCreatedDomainEvent`)
- ✅ Events implement marker interface `IDomainEvent`
- ✅ Include entity ID, timestamp, and relevant business payload
- ❌ Tech-oriented events (use Integration Events for cross-service)
- ❌ Events with serialization concerns in Domain layer (keep Domain pure)

### Raising Events
```csharp
// ✅ CORRECT
public class Order
{
    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void Confirm()
    {
        Status = OrderStatus.Confirmed;
        _domainEvents.Add(new OrderConfirmedDomainEvent(Id, DateTime.UtcNow));
    }
}

// ❌ WRONG
public class OrderService
{
    public void ConfirmOrder(Guid id)
    {
        // Domain logic outside Domain layer
        _serviceBus.Publish(new OrderConfirmedEvent { ... });
    }
}
```

### Collecting & Dispatching
- ✅ Command handler collects domain events after successful operation
- ✅ Dispatches via MediatR notification handlers (`INotificationHandler<IDomainEvent>`)
- ✅ Notification handler publishes to Service Bus via MassTransit
- ✅ Outbox guarantees delivery (same DB transaction as business data)
- ❌ Dispatching events before business data is committed
- ❌ Mixing domain event dispatch with infrastructure concerns in Application layer

---

## G-WEB-BE-07: Outbox Pattern (Reliable Event Publishing)

### Outbox Table
- ✅ `OutboxMessage` table with columns: `Id`, `EventType`, `Payload`, `CorrelationId`, `CreatedAt`, `ProcessedAt`, `Error`
- ✅ Always written in the same EF Core transaction as the business operation (`SaveChangesAsync`)
- ✅ Payload serialized as JSON using `System.Text.Json`
- ✅ Includes `CorrelationId` for end-to-end tracing
- ❌ Publishing events directly from command handlers (risk of data loss if DB succeeds but publish fails)
- ❌ Storing unsupported types in Payload (only JSON-serializable data)

### Outbox Publisher (Background Service)
- ✅ `BackgroundService` polls `OutboxMessage` table every ~1 second for unprocessed records
- ✅ Publishes each message to the correct Service Bus topic/queue via MassTransit
- ✅ Marks `ProcessedAt` on successful delivery (never delete, use soft-mark)
- ✅ Implements exponential backoff on publish failure (2, 4, 8, 16s)
- ✅ Logs every publish attempt with correlation ID
- ❌ Deleting processed messages immediately (retain for auditing at least 7 days)
- ❌ Blocking the main request pipeline to wait for outbox processing

### Idempotency & Deduplication
- ✅ Each outbox message has unique Id
- ✅ Background publisher checks IdempotentProducer/Consumer before publishing
- ✅ Integration consumers implement Inbox pattern to handle duplicate deliveries

---

## G-WEB-BE-08: Background Job Rules

### Hosted Services
- ✅ Use `BackgroundService` or `IHostedService` for periodic/background work
- ✅ Respect `CancellationToken` for graceful shutdown
- ✅ Implement health checks: `IHealthCheck` reports service status
- ✅ Idempotent: running multiple times produces same result
- ✅ Log start/completion/duration for each cycle
- ❌ Long-running synchronous work inside request pipeline
- ❌ Starting external processes or shell commands
- ❌ Ignoring cancellation tokens (blocks shutdown)

### Pattern
```csharp
// ✅ CORRECT
public class OutboxPublisher : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await _service.PublishPendingMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox publish cycle failed");
            }
        }
    }
}
```

---

## G-WEB-BE-09: Sync vs Async Decision Guide

### When to Use Synchronous
- ✅ Client needs immediate response (<500ms)
- ✅ Strong consistency required (no eventual consistency)
- ✅ Simple CRUD without cross-service side effects
- ✅ Read/query operations
- ✅ Client is waiting for the result

### When to Use Asynchronous (Event-Driven)
- ✅ Operation involves multiple services
- ✅ Operation takes longer than 500ms
- ✅ Client does not need immediate response (fire-and-forget)
- ✅ Eventual consistency is acceptable
- ✅ Retry needed on failure (built-in via Service Bus + Outbox)
- ✅ Loose coupling between producer and consumer

### Hybrid Pattern
- ✅ Command returns sync response (201 Created) + publishes domain event
- ✅ Side effects (inventory update, notification, sync) happen async via Service Bus
- ✅ Client polls or uses WebSocket for completion status
- ✅ Saga for multi-step workflows spanning services
