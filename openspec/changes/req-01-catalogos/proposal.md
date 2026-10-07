## Why

El proyecto Mi Minería requiere una administración centralizada y gobernada de los catálogos globales del sistema (Empresa, Formato y Disciplina) para garantizar la estandarización, integridad referencial y control de acceso. Actualmente, estos catálogos no tienen un punto único de gestión, lo que genera riesgos de duplicidad, inconsistencia y falta de trazabilidad. Implementar este módulo es el primer paso funcional del proyecto (REQ_01) y establece la base para que otros módulos dependan de estos catálogos configurados.

## What Changes

- Se introduce un módulo de **Seguridad** con un submódulo **Catálogos** accesible únicamente al Administrador Global.
- El Administrador Global puede crear, editar y eliminar registros en tres catálogos globales:
  - **Empresa**: para identificar a Mi Minería y terceros (clientes, contratistas, proveedores, etc.)
  - **Formato**: para definir tipos de inspección y documentos de control de calidad
  - **Disciplina**: para clasificar áreas técnicas de los proyectos (estructuras, instalaciones, etc.)
- Validaciones de negocio:
  - Anti-duplicado (case-insensitive) en Código y Descripción
  - Bloqueo de edición de código si la empresa/formato/disciplina está en uso
  - Bloqueo de eliminación si está en uso
  - Confirmación antes de eliminar
- UI web con listados filtrados, búsqueda, formularios de creación/edición, e indicadores visuales de estado.

## Capabilities

### New Capabilities

- `global-catalogs`: Gestión centralizada de catálogos Empresa, Formato y Disciplina con CRUD, validaciones anti-duplicado, reglas de bloqueo por uso y control de acceso restringido a Administrador Global.

## Impact

**API Backend** (.NET 10 + CQRS + MediatR):
- 3 nuevos agregados de dominio: `CompanyCatalog`, `FormatCatalog`, `DisciplineCatalog`
- Endpoints CreateCommand, UpdateCommand, DeleteCommand + queries de listado/búsqueda

**Base de datos** (SQL Server 2022):
- Tablas `Catalogs.Company`, `Catalogs.Format`, `Catalogs.Discipline` con índices de unicidad
- Tabla de auditoría `Audit.CatalogChanges`

**Frontend** (Angular 20 zoneless):
- Componente `CatalogManagementComponent` (OnPush + signals)
- Subcomponentes: `CatalogListComponent`, `CatalogFormComponent`
- Servicio `CatalogService` (RxJS 7.8)

**Seguridad**:
- Control de acceso por rol: solo Administrador Global
- Validación de permisos en el backend (AuthorizationPolicy)

**Testing** (Pirámide):
- Unit tests: validaciones de negocio (anti-duplicado, bloqueos)
- Integration tests: E2E CRUD en BD con Testcontainers
- UI tests: interacciones de formulario y búsqueda (70%+ cobertura frontend)

**Guardrails aplicables**:
- G-GLOBAL-01 (Naming: kebab-case archivos, PascalCase clases)
- G-GLOBAL-03 (Security: acceso validado en backend, no secretos)
- G-GLOBAL-06 (Observability: correlation ID en logs, métricas de disponibilidad)
- G-WEB-BE-01 (Hexagonal + CQRS)
- G-WEB-FE-03 (Angular 20 zoneless, OnPush, signals)
- G-DB-02 (Migraciones Script-Only, no EF Core Migrations)
- G-TEST-06 (Testcontainers para pruebas de integración)
