# Arquitectura REQ-01 — Catálogos globales

## Resumen

Módulo de Seguridad > Catálogos que administra Empresa, Formato y Disciplina
exclusivamente para el rol Administrador Global. Sigue Hexagonal + CQRS + MediatR
en backend y Angular 20 zoneless (OnPush + signals) en frontend.

## Capas backend

- `Domain/Catalogs`: agregados (`Company`, `Format`, `Discipline`), value objects
  (`CatalogCode`, `CatalogDescription`), eventos de dominio, excepciones e interfaces
  de repositorio/`IUnitOfWork`. No depende de otras capas (G-WEB-BE-01).
- `Application/Catalogs`: comandos, queries, handlers MediatR y validadores
  FluentValidation. Orquesta casos de uso y valida duplicidad antes de persistir
  (la verificación anti-duplicado vive en los handlers, por la Decisión 5).
- `Infrastructure/Catalogs`: `CatalogDbContext` (EF Core solo ORM de consulta),
  repositorios y `UnitOfWork`. La auditoría se materializa dentro de `SaveChangesAsync`
  con una única llamada a EF Core, dentro de la misma transacción, y guarda
  `OldValue`/`NewValue` para los eventos de actualización (G-GLOBAL-06).
- `Presentation/Catalogs`: `CatalogController` con `[Authorize(Roles="GlobalAdmin")]`,
  middlewares de `X-Correlation-Id` (que además abre un scope de log con el id) y
  transformación de excepciones de dominio a HTTP. Los validadores FluentValidation
  se registran en el composition root.

## Flujo de creación

1. `POST /api/catalogs/companies` → `CreateCompanyCommand`.
2. El handler consulta por código y descripción (case-insensitive) para evitar duplicados.
3. Crea el agregado `Company`, que registra `CompanyCreatedEvent`.
4. `IUnitOfWork.SaveChangesAsync` persiste el agregado y escribe el log en
   `Audit.CatalogChanges` en una única llamada a EF Core.

## Persistencia

- Migraciones Script-Only (G-DB-02) en `src/apps/web/database/Migrations`.
- `v001_create_catalog_tables.sql` crea `Catalogs.Company/Format/Discipline` y
  `Audit.CatalogChanges`, con índices únicos sobre `Code` y `Description`.
- `v001_rollback.sql` revierte el esquema.

## Frontend

- `CatalogManagementComponent` es el contenedor smart (signals, OnPush); combina
  tipo, filtros, paginación y trigger de recarga con `debounceTime(300)`.
- `CatalogSelectorComponent`, `CatalogListComponent` y `CatalogFormComponent` son
  presentacionales con `input()`/`output()`.
- `CatalogListComponent` expone búsqueda por código y descripción; el contenedor
  posee la paginación y el estado de carga.
- `CatalogService` encapsula RxJS y mapea errores HTTP a mensajes de UI.
- `GlobalAdminGuard` valida el claim `roles` del token en el cliente; el backend
  sigue siendo la fuente de verdad de autorización.
