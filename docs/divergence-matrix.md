# Matriz de divergencias local / Azure (G-GLOBAL-08)

| Aspecto | Local (dev/test) | Azure (producción) | Notas |
|---|---|---|---|
| SQL Server | Contenedor Docker `mssql/server:2022-latest` o Testcontainers | Azure SQL | Scripts SQL idénticos (`database/Migrations`) |
| Autenticación SQL | `User Id=sa` + password de desarrollo | Managed Identity (`Authentication=Active Directory Default`) | No se almacenan secretos de producción en config |
| Identidad / JWT | Mock de claims en pruebas de UI | Microsoft Entra ID | `GlobalAdminGuard` lee el mismo claim `roles` |
| Auditoría | `Audit.CatalogChanges` en la BD local | `Audit.CatalogChanges` + App Insights como fallback | Mismo evento de dominio |
| Correlation ID | Middleware `X-Correlation-Id` | Idem + trazabilidad en App Insights | Propaga desde interceptor Angular |
| Observabilidad | Logs estándar | App Insights (métricas G-GLOBAL-06) | Fuera de alcance en sandbox |
