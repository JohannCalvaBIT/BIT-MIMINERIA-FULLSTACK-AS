## 1. Estructura de carpetas y configuración base

- [x] 1.1 Crear estructura de carpetas backend: `src/apps/web/backend/Domain/Catalogs`, `src/apps/web/backend/Application/Catalogs`, `src/apps/web/backend/Infrastructure/Catalogs`, `src/apps/web/backend/Presentation/Catalogs` (G-WEB-BE-01)
- [x] 1.2 Crear estructura de carpetas frontend: `src/apps/web/frontend/src/app/security/catalogs/{shared,components,services}` (G-GLOBAL-01)
- [x] 1.3 Actualizar `appsettings.json` con cadena de conexión de base de datos (local Testcontainers, Azure SQL en producción)
- [x] 1.4 Configurar Docker Compose o Testcontainers para levantar SQL Server local en desarrollo (G-GLOBAL-08, G-TEST-06)

## 2. Backend - Capa de Dominio

- [x] 2.1 Crear clase `Company` (agregado raíz) con propiedades Code, Description, CreatedAt, ModifiedAt (G-WEB-BE-01)
- [x] 2.2 Crear clase `Format` (agregado raíz) con propiedades Code, Description (G-WEB-BE-01)
- [x] 2.3 Crear clase `Discipline` (agregado raíz) con propiedades Code, Description (G-WEB-BE-01)
- [x] 2.4 Implementar value object `CatalogCode` para encapsular validación de código (length, caracteres permitidos)
- [x] 2.5 Implementar value object `CatalogDescription` para encapsular validación de descripción
- [x] 2.6 Crear excepciones de dominio: `DuplicateCodeException`, `DuplicateDescriptionException`, `CatalogInUseException` (G-WEB-BE-03)
- [x] 2.7 Crear eventos de dominio: `CompanyCreatedEvent`, `CompanyUpdatedEvent`, `CompanyDeletedEvent` (y equivalentes para Format, Discipline)
- [x] 2.8 Crear interfaces de repositorio: `ICompanyRepository`, `IFormatRepository`, `IDisciplineRepository` en capa de dominio

## 3. Backend - Capa de Aplicación (CQRS + MediatR)

- [x] 3.1 Crear comando `CreateCompanyCommand` con propiedades Code, Description (G-WEB-BE-01)
- [x] 3.2 Crear command handler `CreateCompanyCommandHandler` que valida duplicidad, crea agregado, publica evento (G-WEB-BE-01)
- [x] 3.3 Crear comando `UpdateCompanyCommand` y handler correspondiente (permitir edición de Code solo si no está en uso, siempre Description)
- [x] 3.4 Crear comando `DeleteCompanyCommand` y handler correspondiente (validar que no esté en uso)
- [x] 3.5 Crear equivalentes de commands para `Format` y `Discipline` (6 commands totales: 3x Create, 3x Update, 3x Delete)
- [x] 3.6 Crear query `GetCompaniesQuery` y handler que retorna lista paginada (G-WEB-BE-01)
- [x] 3.7 Crear query `SearchCompaniesQuery` y handler que filtra por Code/Description (case-insensitive)
- [x] 3.8 Crear equivalentes de queries para `Format` y `Discipline` (6 queries totales)
- [x] 3.9 Implementar interfaz `IUnitOfWork` para manejar transacciones
- [x] 3.10 Crear validadores FluentValidation para cada comando (anti-duplicado verificando repo)

## 4. Backend - Capa de Infraestructura

- [x] 4.1 Crear `CatalogDbContext` derivado de DbContext con DbSets para Company, Format, Discipline, AuditLog (G-WEB-BE-03)
- [x] 4.2 Implementar `CompanyRepository` derivado de `ICompanyRepository` (G-WEB-BE-03)
- [x] 4.3 Implementar `FormatRepository` derivado de `IFormatRepository`
- [x] 4.4 Implementar `DisciplineRepository` derivado de `IDisciplineRepository`
- [x] 4.5 Implementar `UnitOfWork` que coordina repositories y maneja transacciones
- [x] 4.6 Registrar servicios en composition root (`Program.cs`): repositories, DbContext, handlers MediatR, IUnitOfWork (G-GLOBAL-08)
- [x] 4.7 Crear event handler `CatalogChangeAuditedHandler` que escucha eventos de dominio y escribe en tabla `Audit.CatalogChanges` (G-GLOBAL-06)
- [x] 4.8 Configurar connection string: local (Testcontainers) en `appsettings.Local.json`, Azure SQL en `appsettings.Production.json` con Managed Identity (G-GLOBAL-08)

## 5. Backend - Capa de Presentación (API)

- [x] 5.1 Crear `CatalogController` base con autorización `[Authorize(Roles = "GlobalAdmin")]` (G-GLOBAL-03)
- [x] 5.2 Crear endpoints POST `/api/catalogs/companies` (CreateCompanyCommand) con DTO `CreateCompanyDto` (G-WEB-BE-01)
- [x] 5.3 Crear endpoints PUT `/api/catalogs/companies/{id}` (UpdateCompanyCommand)
- [x] 5.4 Crear endpoints DELETE `/api/catalogs/companies/{id}` (DeleteCompanyCommand)
- [x] 5.5 Crear endpoints GET `/api/catalogs/companies` (GetCompaniesQuery, soportar pagination ?skip=0&take=10)
- [x] 5.6 Crear endpoints GET `/api/catalogs/companies/search?code=...&description=...` (SearchCompaniesQuery)
- [x] 5.7 Crear equivalentes de endpoints para `/api/catalogs/formats` y `/api/catalogs/disciplines` (18 endpoints totales)
- [x] 5.8 Implementar error handling middleware que transforma excepciones de dominio a respuestas HTTP (DuplicateCodeException → 400 BadRequest, etc.)
- [x] 5.9 Implementar correlation ID middleware para trazabilidad (G-GLOBAL-06)

## 6. Base de Datos - Migraciones SQL

- [x] 6.1 Crear script `src/apps/web/database/Migrations/v001_create_catalog_tables.sql` que crea tablas `Catalogs.Company`, `Catalogs.Format`, `Catalogs.Discipline` (G-DB-02)
- [x] 6.2 Agregar índices UNIQUE en (Catalogs.Company.Code, Catalogs.Company.Description) y equivalentes para Format, Discipline
- [x] 6.3 Crear tabla `Audit.CatalogChanges` con columnas: Id, EntityType, EntityId, Action, UserId, Timestamp, OldValue, NewValue (G-GLOBAL-06)
- [x] 6.4 Crear script `src/apps/web/database/Migrations/v001_rollback.sql` que revierte v001
- [x] 6.5 Crear script de deployment `src/apps/web/database/run-migrations.ps1` que ejecuta scripts SQL en orden (G-DB-02)

## 7. Frontend - Estructura de componentes

- [x] 7.1 Crear componente `CatalogManagementComponent` en `src/apps/web/frontend/src/app/security/catalogs/` con ChangeDetectionStrategy.OnPush, signal() para estado (G-WEB-FE-03)
- [x] 7.2 Crear subcomponente `CatalogSelectorComponent` en `src/apps/web/frontend/src/app/security/catalogs/components/`
- [x] 7.3 Crear subcomponente `CatalogListComponent` con búsqueda/filtrado (OnPush + signals)
- [x] 7.4 Crear subcomponente `CatalogFormComponent` para crear/editar registros (reactive forms, OnPush)
- [x] 7.5 Crear servicio `CatalogService` en `src/apps/web/frontend/src/app/security/catalogs/services/` con métodos create/update/delete/getList/search (RxJS 7.8)
- [x] 7.6 Crear tipos TypeScript en `src/apps/web/frontend/src/app/security/catalogs/shared/`: `CompanyDto`, `FormatDto`, `DisciplineDto`, `CatalogType` enum

## 8. Frontend - Implementación de formularios y lógica

- [x] 8.1 Implementar formulario reactivo en `CatalogFormComponent` (`src/apps/web/frontend/src/app/security/catalogs/components/catalog-form.component.ts`) con validadores: required, maxLength (code: 150, description: 250)
- [x] 8.2 Conectar submit del formulario a `CatalogService.create()` / `CatalogService.update()`
- [x] 8.3 Mostrar mensajes de error de validación (duplicidad, length) y éxito (G-GLOBAL-03)
- [x] 8.4 Implementar search debounced en `CatalogListComponent` (300ms debounce) para performance
- [x] 8.5 Mostrar indicador "En uso" en listado si un registro está en uso (consultar backend o mostrar campo `IsInUse` en DTO)
- [x] 8.6 Deshabilitar botón "Editar Código" y botón "Eliminar" si registro está en uso
- [x] 8.7 Implementar confirmación de eliminación (modal/dialog con "¿Está seguro?")
- [x] 8.8 Manejar errores HTTP en `CatalogService` y propagar mensajes claros a la UI (G-GLOBAL-03)

## 9. Frontend - Acceso y autenticación

- [x] 9.1 Crear guard `GlobalAdminGuard` en `src/apps/web/frontend/src/app/security/catalogs/shared/` que verifica si el usuario tiene rol "GlobalAdmin" (leer claims del token)
- [x] 9.2 Proteger ruta `/security/catalogs` con `GlobalAdminGuard` en el routing module
- [x] 9.3 Ocultar enlace a "Catálogos" en menú si usuario no tiene rol GlobalAdmin (UX mejora)
- [x] 9.4 Enviar correlation ID en headers de requests HTTP (propagar desde el frontend) (G-GLOBAL-06)

## 10. Testing - Unidad (Backend)

- [x] 10.1 Crear tests unitarios para `Company` agregado: constructores, métodos de validación, eventos de dominio
- [x] 10.2 Crear tests unitarios para value objects `CatalogCode`, `CatalogDescription`: validación de length, caracteres
- [x] 10.3 Crear tests unitarios para `CreateCompanyCommandHandler`: happy path, duplicado, validación fallida
- [x] 10.4 Crear tests unitarios para `UpdateCompanyCommandHandler`: editar no en uso, editar en uso (código bloqueado), validación
- [x] 10.5 Crear tests unitarios para `DeleteCompanyCommandHandler`: eliminar no en uso, bloqueo si en uso
- [x] 10.6 Crear tests unitarios para queries: `GetCompaniesQuery`, `SearchCompaniesQuery`
- [x] 10.7 Crear tests unitarios para FluentValidation validators
- [x] 10.8 Crear tests unitarios equivalentes para `Format` y `Discipline` (18 test classes, 75%+ cobertura target)

## 11. Testing - Integración (Backend)

- [ ] 11.1 Crear fixture de test que levanta Testcontainers SQL Server y migra schema (G-TEST-06)
- [ ] 11.2 Crear tests de integración para `CompanyRepository`: Create, Read, Update, Delete con BD real
- [ ] 11.3 Crear tests de integración para `CreateCompanyCommandHandler`: validar que el registro se persiste y evento se publica
- [ ] 11.4 Crear tests de integración para anti-duplicado: insertar, intentar crear duplicado, verificar excepción
- [ ] 11.5 Crear tests de integración para auditoría: crear/editar/eliminar, verificar que se escriben en `Audit.CatalogChanges`
- [ ] 11.6 Crear tests de integración para UnitOfWork: transacción exitosa, rollback
- [ ] 11.7 Crear tests de integración equivalentes para `Format` y `Discipline`

## 12. Testing - Endpoints (Backend)

- [ ] 12.1 Crear tests de API para POST `/api/catalogs/companies` con autorización: success (200), no autorizado (403), validación fallida (400)
- [ ] 12.2 Crear tests de API para PUT `/api/catalogs/companies/{id}`: editar, bloqueo de código si en uso
- [ ] 12.3 Crear tests de API para DELETE `/api/catalogs/companies/{id}`: eliminar, bloqueo si en uso
- [ ] 12.4 Crear tests de API para GET `/api/catalogs/companies`: paginación, filtrado
- [ ] 12.5 Crear tests de API equivalentes para `/formats` y `/disciplines`

## 13. Testing - Frontend (Angular)

- [x] 13.1 Crear tests unitarios para `CatalogFormComponent`: reactive form, validaciones, submit
- [x] 13.2 Crear tests unitarios para `CatalogListComponent`: listar, buscar, habilitar/deshabilitar acciones
- [x] 13.3 Crear tests unitarios para `CatalogService`: métodos create/update/delete retornan observables correctos
- [x] 13.4 Crear tests unitarios para `GlobalAdminGuard`: retorna true si GlobalAdmin, false si no (70%+ cobertura target)
- [x] 13.5 Crear tests e2e (Cypress o Playwright): flujo completo crear empresa, editar, buscar, eliminar
- [x] 13.6 Crear tests e2e para flujo bloqueado: crear empresa, marcar como en uso, intentar editar código/eliminar

## 14. Documentación y validación

- [ ] 14.1 Documentar la arquitectura CQRS + MediatR en `docs/architecture/req-01-catalogs-architecture.md` con ejemplos
- [ ] 14.2 Crear la documentación de divergencias local/Azure en `docs/divergence-matrix.md` (G-GLOBAL-08)
- [ ] 14.3 Ejecutar `bit validate` para validar specs, design y tasks contra guardrails
- [ ] 14.4 Crear PR con todos los cambios, incluir enlace a proposal/specs/design/tasks en descripción

## 15. Integración y deployment

- [ ] 15.1 Crear smoke test que verifica CRUD de catalogs en ambiente Azure (Managed Identity real) (G-GLOBAL-08)
- [ ] 15.2 Ejecutar script `run-migrations.ps1` en staging: verificar que tablas se crean y indices funcionan
- [ ] 15.3 Crear rollback plan: script v001_rollback.sql
- [ ] 15.4 Verificar logs de auditoría en Application Insights post-deployment (G-GLOBAL-06)
- [ ] 15.5 Registrar actividad de implementación en `docs/metricas/guardrail_activity_log.csv` (tiempo estimado vs real, guardrails aplicados, resultado OK)
