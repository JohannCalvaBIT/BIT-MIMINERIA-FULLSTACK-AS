# GUARDRAILS - Database Modeling & Migration Scripts (Expert Level)

**Rol**: Database Architect / Backend Developer  
**Stack**: SQL Server / Azure SQL / PostgreSQL  
**Última actualización**: 2026-09-15

> Multi-tenant NO es una característica fija del stack — es una decisión de arquitectura por proyecto. Las reglas de aislamiento por tenant (G-DB-01 fila "Tenant Isolation", G-DB-06) solo aplican si el proyecto en cuestión es multi-tenant.

---

## G-DB-01: Database Modeling Standards (Mandatory)

### Entity-Relationship Principles

| Rule | Implementation |
|------|---------------|
| **Primary Keys** | `UNIQUEIDENTIFIER` (UUID v7/GUID) + `DEFAULT NEWSEQUENTIALID()` / `gen_random_uuid()` |
| **Business Keys** | Natural key `UNIQUE` constraint + surrogate UUID PK |
| **Timestamps** | `CreatedAt DATETIME2(3) DEFAULT SYSUTCDATETIME()`, `UpdatedAt DATETIME2(3) NULL` |
| **Soft Delete** | `DeletedAt DATETIME2(3) NULL` + filtered index `WHERE DeletedAt IS NULL` |
| **Row Version** | `RowVersion ROWVERSION` (SQL Server) / `xmin` (PostgreSQL) for optimistic concurrency |
| **Tenant Isolation** | `TenantId UNIQUEIDENTIFIER NOT NULL` es obligatorio en toda tabla **solo si el proyecto es multi-tenant** (ver G-DB-06) |

### Naming Conventions (Strict)

```sql
-- Tables: PascalCase, plural (business domain aligned)
Orders, OrderItems, PaymentTransactions, CustomerProfiles

-- Columns: PascalCase
OrderId, CustomerId, TotalAmount, CreatedAt, UpdatedAt, DeletedAt

-- Indexes: IX_{Table}_{Column(s)}
IX_Orders_CustomerId_Status, IX_OrderItems_OrderId

-- Foreign Keys: FK_{ChildTable}_{ParentTable}
FK_OrderItems_Orders, FK_Orders_Customers

-- Check Constraints: CK_{Table}_{Rule}
CK_Orders_TotalAmount_Positive, CK_OrderItems_Quantity_GT_Zero

-- Default Constraints: DF_{Table}_{Column}
DF_Orders_CreatedAt, DF_Orders_Status_Pending

-- Triggers: TR_{Table}_{Action}
TR_Orders_UpdatedAt, TR_OrderItems_ValidateStock
```

### Anti-Patterns (Prohibited)

| ❌ Anti-Pattern | ✅ Correct |
|-----------------|------------|
| `Id INT IDENTITY` | `Id UNIQUEIDENTIFIER DEFAULT NEWSEQUENTIALID()` |
| `CreatedDate DATETIME` | `CreatedAt DATETIME2(3) DEFAULT SYSUTCDATETIME()` |
| `IsDeleted BIT` | `DeletedAt DATETIME2(3) NULL` + filtered index |
| `Status VARCHAR(50)` | `Status TINYINT` + lookup table + CHECK constraint |
| `Data JSON` (unstructured) | Structured columns + JSON only for extensibility |
| Composite PK | Surrogate UUID PK + Unique Business Key |
| `SELECT *` | Explicit column list ALWAYS |

---

## G-DB-02: Migration Script Standards (Script-Only, No ORM Migrations)

**Esta es la autoridad para migraciones de esquema en todo el proyecto** — `guardrails-03-web-backend.md` (G-WEB-BE-02) usa EF Core solo como ORM de consulta (DbContext, LINQ) y remite aquí para todo lo que sea evolución de esquema.

### Script Organization

```
database/
├── 000_Baseline/
│   ├── 001_CreateSchemas.sql
│   ├── 002_CreateCoreTables.sql
│   ├── 003_CreateIndexes.sql
│   └── 004_CreateConstraints.sql
│
├── 001_Initial/
│   ├── 001_CreateOrdersSchema.sql
│   ├── 002_CreateCustomersSchema.sql
│   └── 003_CreateReferenceData.sql
│
├── 002_Feature_OrderProcessing/
│   ├── 001_AddOrderPriorityColumn.sql
│   ├── 002_CreateOrderEventsTable.sql
│   └── 003_AddOrderPriorityIndex.sql
│
├── 003_Hotfix_PaymentTimeout/
│   └── 001_ExtendPaymentTimeoutDefault.sql
│
└── _meta/
    ├── SchemaVersion.sql          -- Current version tracking
    ├── DeployOrder.json           -- Dependency graph
    └── RollbackPlan.md            -- Per-release rollback steps
```

### Script Template (Mandatory)

```sql
-- =====================================================
-- Migration: 001_AddOrderPriorityColumn.sql
-- Feature: Order Priority Processing
-- Author: [Name]
-- Date: 2026-07-17
-- Ticket: (opcional, id del issue tracker que use el equipo)
-- Description: Add Priority column to Orders for expedited processing
-- =====================================================

-- PRE-CONDITIONS CHECK
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Orders' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    RAISERROR('Pre-condition failed: Orders table does not exist', 16, 1);
    RETURN;
END

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Orders') AND name = 'Priority')
BEGIN
    PRINT 'Column Priority already exists. Skipping.';
    RETURN;
END

BEGIN TRANSACTION;

    -- 1. ADD COLUMN WITH DEFAULT (non-blocking for existing rows)
    ALTER TABLE dbo.Orders
    ADD Priority TINYINT NOT NULL
        CONSTRAINT DF_Orders_Priority DEFAULT (0);  -- 0=Normal, 1=High, 2=Critical

    -- 2. ADD CHECK CONSTRAINT (validates existing + future data)
    ALTER TABLE dbo.Orders
    ADD CONSTRAINT CK_Orders_Priority_Range
    CHECK (Priority IN (0, 1, 2));

    -- 3. BACKFILL STRATEGY (if needed, in batches)
    -- UPDATE TOP (10000) dbo.Orders SET Priority = 0 WHERE Priority IS NULL;

    -- 4. CREATE INDEX (ONLINE = ON for production)
    CREATE NONCLUSTERED INDEX IX_Orders_Priority_Status_CreatedAt
    ON dbo.Orders (Priority, Status, CreatedAt)
    WHERE DeletedAt IS NULL
    WITH (ONLINE = ON, DATA_COMPRESSION = PAGE);

    -- 5. UPDATE STATISTICS
    UPDATE STATISTICS dbo.Orders IX_Orders_Priority_Status_CreatedAt WITH FULLSCAN;

COMMIT TRANSACTION;

-- POST-CONDITIONS VALIDATION
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Orders') AND name = 'Priority')
BEGIN
    RAISERROR('Post-condition failed: Priority column not created', 16, 1);
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Orders_Priority_Status_CreatedAt')
BEGIN
    RAISERROR('Post-condition failed: Index not created', 16, 1);
END

PRINT 'Migration 001_AddOrderPriorityColumn.sql completed successfully.';
```

### Rollback Script Template (Required per Migration)

```sql
-- =====================================================
-- Rollback: 001_AddOrderPriorityColumn_Rollback.sql
-- Reverts: 001_AddOrderPriorityColumn.sql
-- =====================================================

BEGIN TRANSACTION;

    -- Drop index first (depends on column)
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Orders_Priority_Status_CreatedAt')
    BEGIN
        DROP INDEX IX_Orders_Priority_Status_CreatedAt ON dbo.Orders;
    END

    -- Drop constraint
    IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Orders_Priority_Range')
    BEGIN
        ALTER TABLE dbo.Orders DROP CONSTRAINT CK_Orders_Priority_Range;
    END

    -- Drop default constraint
    IF EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Orders_Priority')
    BEGIN
        ALTER TABLE dbo.Orders DROP CONSTRAINT DF_Orders_Priority;
    END

    -- Drop column
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Orders') AND name = 'Priority')
    BEGIN
        ALTER TABLE dbo.Orders DROP COLUMN Priority;
    END

COMMIT TRANSACTION;

PRINT 'Rollback completed successfully.';
```

---

## G-DB-03: Deployment Pipeline (Script-Based Only)

### CI/CD Gates

```yaml
# .github/workflows/database-deploy.yml
jobs:
  validate-scripts:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Validate SQL Syntax (sqlfluff)
        run: sqlfluff lint database/ --rules L001,L003,L010,L014,L016,L029,L031,L036,L044
      
      - name: Check Naming Conventions
        run: |
          # Custom script to enforce naming standards
          ./scripts/validate-db-naming.sh database/
      
      - name: Verify Rollback Scripts Exist
        run: |
          for f in database/**/[0-9]*.sql; do
            rollback="${f%.sql}_Rollback.sql"
            if [[ ! -f "$rollback" ]]; then
              echo "❌ Missing rollback: $rollback"
              exit 1
            fi
          done

  deploy-staging:
    needs: validate-scripts
    environment: staging
    steps:
      - name: Deploy to Staging (Transactional)
        run: |
          sqlcmd -S $(STAGING_SERVER) -d $(STAGING_DB) -U $(ADMIN_USER) -P $(ADMIN_PASS) \
            -i database/000_Baseline/001_CreateSchemas.sql \
            -i database/000_Baseline/002_CreateCoreTables.sql \
            -b -V16  # Batch mode, fail on severity >= 16
      
      - name: Run Integration Tests
        run: dotnet test tests/IntegrationTests --filter "Category=Database"
      
      - name: Validate Schema (Schema Compare)
        run: |
          sqlpackage /Action:DeployReport /SourceFile:bin/Debug/dacpac /TargetConnectionString:"$(STAGING_CONN)" /OutputPath:schema-drift.xml

  deploy-production:
    needs: deploy-staging
    environment: production
    steps:
      - name: Blue-Green Deploy (Read Replica Switchover)
        run: |
          # 1. Deploy to passive replica
          # 2. Validate
          # 3. Failover
          ./scripts/blue-green-deploy.sh production
      
      - name: Post-Deploy Validation
        run: |
          # Smoke tests against production
          ./scripts/smoke-tests.sh production
```

### Execution Rules

| Rule | Enforcement |
|------|-------------|
| **Single Transaction** | Each script runs in explicit `BEGIN/COMMIT TRANSACTION` |
| **Idempotent** | `IF NOT EXISTS` / `IF EXISTS` guards on all DDL |
| **Online Operations** | `WITH (ONLINE = ON)` for indexes on prod tables > 1GB |
| **Batch Large Updates** | `TOP (10000)` loops with `WAITFOR DELAY '00:00:00.1'` |
| **No Data Loss** | Rollback script MUST exist and be tested |
| **Schema Version** | `INSERT INTO SchemaVersion (Version, AppliedAt, ScriptName) VALUES (...)` |
| **Zero Downtime** | Additive changes only (new columns nullable/defaults, new tables) |

---

## G-DB-04: High-Concurrency Transactional Schema Patterns

### Optimistic Concurrency (Default)

```sql
-- Table with rowversion for optimistic locking
CREATE TABLE dbo.Orders (
    Id              UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId        UNIQUEIDENTIFIER NOT NULL,
    CustomerId      UNIQUEIDENTIFIER NOT NULL,
    OrderNumber     NVARCHAR(50)     NOT NULL,
    Status          TINYINT          NOT NULL DEFAULT (0),
    TotalAmount     DECIMAL(18,2)    NOT NULL,
    Priority        TINYINT          NOT NULL DEFAULT (0),
    Version         ROWVERSION       NOT NULL,  -- SQL Server
    -- Version        BIGINT           NOT NULL DEFAULT (0), -- PostgreSQL: use xmin or trigger
    CreatedAt       DATETIME2(3)     NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt       DATETIME2(3)     NULL,
    DeletedAt       DATETIME2(3)     NULL,
    
    CONSTRAINT PK_Orders PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Orders_Tenant_OrderNumber UNIQUE (TenantId, OrderNumber),
    CONSTRAINT CK_Orders_Status_Valid CHECK (Status IN (0,1,2,3,4)), -- Pending,Confirmed,Shipped,Delivered,Cancelled
    CONSTRAINT CK_Orders_TotalAmount_Positive CHECK (TotalAmount >= 0),
    CONSTRAINT CK_Orders_Priority_Range CHECK (Priority IN (0,1,2))
);

-- Filtered index for active orders (excludes soft-deleted)
CREATE NONCLUSTERED INDEX IX_Orders_Tenant_Status_CreatedAt
ON dbo.Orders (TenantId, Status, CreatedAt DESC)
WHERE DeletedAt IS NULL
WITH (DATA_COMPRESSION = PAGE);
```

### Pessimistic Locking (Critical Sections Only)

```sql
-- Stored procedure for critical inventory reservation
CREATE PROCEDURE dbo.ReserveInventory
    @OrderId       UNIQUEIDENTIFIER,
    @ProductId     UNIQUEIDENTIFIER,
    @Quantity      INT,
    @TenantId      UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT, XACT_ABORT ON;
    
    BEGIN TRANSACTION;
    
    -- UPDLOCK + HOLDLOCK = serializable range lock
    DECLARE @Available INT;
    SELECT @Available = AvailableQuantity
    FROM dbo.Inventory WITH (UPDLOCK, HOLDLOCK, ROWLOCK)
    WHERE ProductId = @ProductId AND TenantId = @TenantId;
    
    IF @Available < @Quantity
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 50001, 'Insufficient inventory', 1;
    END
    
    UPDATE dbo.Inventory
    SET AvailableQuantity -= @Quantity,
        UpdatedAt = SYSUTCDATETIME()
    WHERE ProductId = @ProductId AND TenantId = @TenantId;
    
    INSERT INTO dbo.InventoryReservations (Id, OrderId, ProductId, Quantity, TenantId, CreatedAt)
    VALUES (NEWSEQUENTIALID(), @OrderId, @ProductId, @Quantity, @TenantId, SYSUTCDATETIME());
    
    COMMIT TRANSACTION;
END;
```

### Outbox Pattern (Transactional Messaging)

```sql
-- Outbox table (same transaction as business data)
CREATE TABLE dbo.OutboxMessages (
    Id              UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId        UNIQUEIDENTIFIER NOT NULL,
    EventType       NVARCHAR(200)    NOT NULL,
    Payload         NVARCHAR(MAX)    NOT NULL,  -- JSON
    CorrelationId   UNIQUEIDENTIFIER NULL,
    CausationId     UNIQUEIDENTIFIER NULL,
    CreatedAt       DATETIME2(3)     NOT NULL DEFAULT SYSUTCDATETIME(),
    ProcessedAt     DATETIME2(3)     NULL,
    RetryCount      TINYINT          NOT NULL DEFAULT (0),
    LastError       NVARCHAR(MAX)    NULL,
    
    CONSTRAINT PK_OutboxMessages PRIMARY KEY CLUSTERED (Id)
);

-- Index for publisher (unprocessed, ordered)
CREATE NONCLUSTERED INDEX IX_OutboxMessages_Unprocessed
ON dbo.OutboxMessages (CreatedAt)
WHERE ProcessedAt IS NULL
WITH (DATA_COMPRESSION = PAGE);

-- Publisher (background service) - polls every 500ms
-- SELECT TOP (500) * FROM OutboxMessages 
-- WHERE ProcessedAt IS NULL AND TenantId = @TenantId
-- ORDER BY CreatedAt
-- FOR UPDATE SKIP LOCKED  -- PostgreSQL / SQL Server 2022+
```

---

## G-DB-05: Performance & Indexing Strategy

### Index Design Rules

| Scenario | Index Strategy |
|----------|---------------|
| **PK Lookup** | Clustered PK (UUID sequential) |
| **Tenant + Status + Date** | Nonclustered `(TenantId, Status, CreatedAt DESC) WHERE DeletedAt IS NULL` |
| **FK Joins** | Nonclustered on FK column(s) |
| **Search/Filter** | Nonclustered INCLUDE for covering |
| **Soft Delete** | ALWAYS filtered index `WHERE DeletedAt IS NULL` |
| **High Cardinality** | Consider columnstore for analytics |

### Compression & Partitioning

```sql
-- Page compression for OLTP tables (default)
ALTER INDEX ALL ON dbo.Orders REBUILD WITH (DATA_COMPRESSION = PAGE);

-- Partition large tables by date (monthly)
CREATE PARTITION FUNCTION PF_Orders_ByMonth (DATETIME2(3))
AS RANGE RIGHT FOR VALUES (
    '2026-01-01', '2026-02-01', '2026-03-01', '2026-04-01', '2026-05-01', '2026-06-01',
    '2026-07-01', '2026-08-01', '2026-09-01', '2026-10-01', '2026-11-01', '2026-12-01'
);

CREATE PARTITION SCHEME PS_Orders_ByMonth
AS PARTITION PF_Orders_ByMonth ALL TO ([PRIMARY]);

-- Apply to table (requires rebuild)
CREATE TABLE dbo.Orders (
    -- ... columns ...
) ON PS_Orders_ByMonth(CreatedAt);
```

---

## G-DB-06: Multi-Tenant Data Isolation

> **Alcance condicional** — aplica solo si el proyecto es multi-tenant (varios clientes/organizaciones comparten la misma base de datos). Un proyecto de un solo tenant no necesita `TenantId`, la security policy, ni el resto de esta sección.

### Shared Database, Shared Schema (Row-Level Security)

```sql
-- Security Policy (enforced at engine level)
CREATE SECURITY POLICY TenantIsolationPolicy
ADD FILTER PREDICATE dbo.fn_TenantFilter(TenantId) ON dbo.Orders,
ADD BLOCK PREDICATE dbo.fn_TenantFilter(TenantId) ON dbo.Orders
WITH (STATE = ON);

-- Inline TVF for predicate (optimal performance)
CREATE FUNCTION dbo.fn_TenantFilter(@TenantId UNIQUEIDENTIFIER)
RETURNS TABLE
WITH SCHEMABINDING
AS
RETURN SELECT 1 AS Result
WHERE @TenantId = SESSION_CONTEXT(N'TenantId')  -- Set via sp_set_session_context
   OR SESSION_CONTEXT(N'TenantId') IS NULL;     -- Allow admin bypass
```

### Connection Context Setup (Application)

```csharp
// In DbContext.SaveChangesAsync or middleware
await context.Database.ExecuteSqlRawAsync(
    "EXEC sp_set_session_context @key = N'TenantId', @value = @tenantId",
    new SqlParameter("@tenantId", currentTenant.Id));
```

---

## G-DB-07: Validation Checklist (Per Migration PR)

```markdown
## Database Migration Checklist

### Schema Design
- [ ] UUID PK with `NEWSEQUENTIALID()` / `gen_random_uuid()`
- [ ] `TenantId` on ALL tables (NOT NULL)
- [ ] `CreatedAt` `DATETIME2(3) DEFAULT SYSUTCDATETIME()`
- [ ] `UpdatedAt` nullable + trigger/auto-update
- [ ] `DeletedAt` nullable + filtered indexes
- [ ] `RowVersion` for optimistic concurrency
- [ ] Business key `UNIQUE` constraint
- [ ] CHECK constraints for domain rules
- [ ] Proper data types (DECIMAL for money, TINYINT for enums)

### Indexing
- [ ] Clustered PK on UUID
- [ ] Filtered indexes `WHERE DeletedAt IS NULL` on all tenant tables
- [ ] Nonclustered on FK columns
- [ ] Covering indexes for hot queries (INCLUDE)
- [ ] `DATA_COMPRESSION = PAGE` on all indexes
- [ ] `ONLINE = ON` for production index creates

### Migration Script
- [ ] Single transaction (`BEGIN/COMMIT TRANSACTION`)
- [ ] Idempotent (`IF NOT EXISTS` / `IF EXISTS`)
- [ ] Rollback script exists and tested
- [ ] No `SELECT *`, no destructive ops without data migration
- [ ] Batch updates for large tables (>100k rows)
- [ ] Statistics update after index creation

### Validation
- [ ] Script runs on clean DB (baseline)
- [ ] Script runs on current prod schema (idempotent)
- [ ] Rollback restores exact prior state
- [ ] Performance tested with production data volume
- [ ] Zero-downtime: additive only, no blocking locks > 5s
```

---

## G-DB-08: Tooling Standards

| Tool | Purpose | Version |
|------|---------|---------|
| **SQLFluff** | Linting/formatting | 3.0+ |
| **SQLPackage** | Schema compare/dacpac | Latest |
| **tSQLt** | Unit testing (stored procs) | 1.0+ |
| **Flyway / DbUp** | Script execution orchestration | Latest |
| **SchemaSpy** | Documentation generation | Latest |
| **DataGrip / Azure Data Studio** | IDE | Latest |

---

## Referencias

- [SQL Server Index Design Guide](https://learn.microsoft.com/sql/relational-databases/indexes/)
- [Azure SQL Performance Guidelines](https://learn.microsoft.com/azure/azure-sql/database/)
- [Optimistic Concurrency Patterns](https://learn.microsoft.com/ef/core/saving/concurrency)
- [Row-Level Security](https://learn.microsoft.com/sql/relational-databases/security/row-level-security)