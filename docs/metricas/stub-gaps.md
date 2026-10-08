# Stub gaps — comandos y tareas no ejecutables en el sandbox

- **`bit exec start/end`**: el stub acepta `exec start/end` sin reportar error
  visible y sin escribir la fila esperada en `execution_log.ndjson`; se invoca
  de todas formas como pide el skill. No bloquea el trabajo.
- **`bit list` / `bit validate` / `bit doctor`**: no implementados por el stub.
  Se omite la validación automática de OpenSpec contra guardrails (tarea 14.3).
- **14.4 (crear PR)**: no hay capacidad de crear pull requests desde el sandbox;
  los cambios se dejan en `feature/agentsky-apply-req-01-catalogos` y se hace
  `git push origin HEAD`.
- **15.1 (smoke test Azure)**: no hay entorno Azure ni Managed Identity real.
- **15.2 (`run-migrations.ps1` en staging)**: no hay SQL Server ni staging.
- **15.4 (App Insights)**: no hay App Insights configurado ni Azure disponible.
- **15.5 (registro de actividad)**: se cubre con `bit exec end --log-activity`
  y `docs/metricas/guardrail_activity_log.csv`.
- **15.3**: duplica la tarea 6.4 (`v001_rollback.sql`), ya implementada en la
  Sección 6; no se marca una segunda vez para no falsear progreso.
- **11.1–11.7 (integración)**: se escribió `Tests/Integration/CatalogIntegrationTests.cs`
  y `CatalogTestcontainersFixture.cs` (Testcontainers + repositorio) y compilan, pero no
  se ejecutan porque el sandbox no tiene Docker ni SQL Server. Quedan con `[Explicit]`.
- **12.1–12.5 (endpoints)**: se añadió `Tests/Api/CatalogApiTests.cs` con `WebApplicationFactory`,
  pero no se ejecuta por la misma falta de infraestructura. Queda `[Explicit]`.
