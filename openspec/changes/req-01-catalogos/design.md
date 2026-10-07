## Context

El proyecto Mi Minería utiliza una arquitectura Hexagonal + CQRS + MediatR en el backend (.NET 10) y Angular 20 zoneless en el frontend. Este es el primer módulo de negocio que se implementa (REQ_01), estableciendo patrones que otros módulos heredarán. Los catálogos globales (Empresa, Formato, Disciplina) son configuraciones centrales que no tienen dependencia de proyectos específicos, lo que las convierte en una buena candidata para iterar sobre la arquitectura sin riesgo de impacto en cadena.

El desarrollo es totalmente emulado localmente (SQL Server en contenedor, sin acceso a Azure). La autenticación y autorización usan Microsoft Entra ID como IdP delegado (G-GLOBAL-03, G-WEB-BE-04).

## Goals / Non-Goals

**Goals:**
- Implementar CRUD de catálogos siguiendo Hexagonal + CQRS + MediatR sin desviaciones
- Validar la pirámide de testing (unit, integration, e2e) con cobertura mínima (75% backend, 70% frontend)
- Establecer patrones de acceso a datos (Repository Pattern + Script-Only migrations)
- Demostrar zoneless Angular con OnPush + signals
- Crear la base para que otros módulos reutilicen y extiendan estos patrones
- Asegurar auditoría y trazabilidad en todas las operaciones

**Non-Goals:**
- Implementar UI responsiva para mobile (la UI es web-first; mobile puede venir después)
- Crear APIs batch/bulk (cada operación es individual)
- Implementar import/export masivo en esta iteración (REQ_35 lo hace; aquí solo CRUD)
- Cambiar tecnología de identidad (siempre es Entra ID)

## Decisions

### Decisión 1: Separación clara de los tres catálogos en el dominio

**Decisión**: Crear tres agregados de dominio independientes (`CompanyCatalog`, `FormatCatalog`, `DisciplineCatalog`) en lugar de un agregado genérico único.

**Rationale**: 
- Cada catálogo tiene reglas de negocio idénticas pero entidades distintas. Separarlos evita acoplamiento futuro si sus comportamientos divergen (p.ej., si Empresa requiere un campo adicional en el futuro).
- Cada agregado tendrá su propio repository, que es más limpio que un repository genérico con discriminador.
- MediatR handlers serán específicos por entidad, lo que reduce la complejidad de los handlers genéricos.

**Alternativa rechazada**: Un agregado genérico `CatalogEntry<T>` con discriminador. Sería más compacto, pero haría más difícil validar reglas específicas por entidad.

### Decisión 2: CQRS split estricto con MediatR

**Decisión**: Usar MediatR `IRequestHandler<CreateCompanyCommand>`, `IRequestHandler<UpdateCompanyCommand>`, `IRequestHandler<DeleteCompanyCommand>` para escritura y `IRequestHandler<GetCompaniesQuery>`, `IRequestHandler<SearchCompaniesQuery>` para lectura.

**Rationale**:
- Garantiza el contrato de la arquitectura (G-WEB-BE-01).
- Hace fácil auditoria: cada handler puede loguear el comando recibido y el resultado.
- Permite agregar validaciones, autorizaciones y compensaciones (saga) sin tocar los endpoints.

**Alternativa rechazada**: Direct repository injection en controllers. Más simple, pero viola la capa de application y mezcla concerns.

### Decisión 3: Scripts SQL versionados en lugar de EF Core Migrations

**Decisión**: Todas las migraciones de base de datos serán scripts SQL versionados (v001_create_catalogs.sql, v002_add_audit_table.sql, etc.) ejecutados por un script de deployment, NO por `dotnet ef migrations apply` (G-DB-02).

**Rationale**:
- Los scripts son auditables y versionables directamente en el repo.
- Permiten control fino sobre índices, permisos, constraints—cosas que EF Core Migrations simplifica demasiado.
- En equipos grandes, es más fácil reviewear un `.sql` que una migración de C#.

**Alternativa rechazada**: EF Core Migrations. Más rápido de escribir, pero ata el versionado de la BD al versionado del ORM, lo que complica rollbacks.

### Decisión 4: Repository Pattern con unit of work

**Decisión**: Cada agregado tendrá un repository (`ICompanyCatalogRepository`, `IFormatCatalogRepository`, `IDisciplineCatalogRepository`). Las transacciones se manejan con `IUnitOfWork` inyectado en el handler de aplicación.

**Rationale**:
- Limpia la separación entre Application e Infrastructure.
- El UoW maneja el rollback si algo falla en el dominio, sin que la aplicación tenga que saber de DbContext.

### Decisión 5: Validación anti-duplicado en el dominio, no en la base de datos sola

**Decisión**: El command handler consultará el repositorio (`repository.GetByCode(code)`) antes de crear. Si existe, lanzará una domain exception (`DuplicateCodeException`). El constraint unique en la BD es defensa en profundidad.

**Rationale**:
- Las excepciones de dominio son manejables y se pueden transformar en respuestas HTTP claras.
- Los constraints en BD capturan race conditions; la lógica de dominio captura el flujo normal.
- Las pruebas unitarias pueden verificar el comportamiento sin tener que mockear la BD.

### Decisión 6: Auditoría en un handler de aplicación

**Decisión**: Cada command (Create, Update, Delete) genera un evento de dominio (`CatalogCreated`, `CatalogUpdated`, `CatalogDeleted`). Un handler de aplicación escucha estos eventos y escribe en la tabla `Audit.CatalogChanges` (G-GLOBAL-06).

**Rationale**:
- Desacopla la auditoría de la lógica de negocio.
- Si los eventos fallaran, no interrumpen la transacción principal (event handlers son best-effort).
- Es más fácil testear auditoría por separado.

### Decisión 7: Frontend Zoneless + Signals con OnPush

**Decisión**: Todos los componentes (`catalog-management.component.ts`, `catalog-list.component.ts`, `catalog-form.component.ts`) usan `changeDetection: ChangeDetectionStrategy.OnPush`, state en `signal()`, y computed properties en `computed()`. Cero zone.js (G-WEB-FE-03).

**Rationale**:
- Angle 20 zoneless es requisito del proyecto (CLAUDE.md).
- Signals permiten change detection predecible sin zonas.
- OnPush + signals es más performante que zone.js + default detection.

**Alternativa rechazada**: Usar RxJS subjects en lugar de signals. Signals son más simples y evitan subscription leaks si se usan bien.

### Decisión 8: Autorización por rol en el backend, verificación en la UI

**Decisión**: El backend usa `[Authorize(Roles = "GlobalAdmin")]` en los endpoints. El frontend oculta el módulo Catálogos si el usuario no tiene el rol (lectura de claims del token).

**Rationale**:
- El backend es la fuente de verdad de seguridad.
- La UI hides el módulo para mejorar UX, pero no confía en eso.

### Decisión 9: Desarrollo local con Testcontainers

**Decisión**: Las pruebas de integración usan Testcontainers para levantar un SQL Server en contenedor, no una instancia compartida. Cada test corre en aislamiento (G-TEST-06, G-GLOBAL-08).

**Rationale**:
- Cada developer y el CI tienen el mismo BD state inicial.
- No hay conflictos de datos entre pruebas locales y en el CI.
- Las migraciones se pueden probar de verdad antes de producción.

## Risks / Trade-offs

| Riesgo | Impacto | Mitigación |
|--------|---------|-----------|
| Race condition: dos admins crean la misma empresa simultáneamente | Duplicado en BD a pesar de la validación | Constraint UNIQUE en BD + reintento del client si obtiene violación |
| Migración SQL manual en producción es propensa a errores | Downtime, data loss | Script de rollback correspondiente; approval requerido antes de ejecutar; test de migraciones en staging |
| Zoneless + signals es nuevo en el equipo | Curva de aprendizaje | Documentación clara en CLAUDE.md + ejemplos en este módulo; code review enfocado en patterns |
| Auditoría asincrónica puede perderse si el proceso muere post-commit | Eventos de auditoría no se escriben | Event sourcing completo (fuera de scope); por ahora, logging en Application Insights como fallback |

## Migration Plan

**Fase 1: Local Development (Sprint 1)**
- Crear estructura de carpetas backend + frontend
- Implementar agregados de dominio + handlers CQRS
- Crear scripts SQL de creación de tablas + auditoría
- Desarrollar componentes Angular + servicios
- Escribir tests unitarios + integración

**Fase 2: Testing & Validation (Sprint 2)**
- Ejecutar pirámide de testing completa
- Validar smoke tests en ambiente Azure (Managed Identity, etc.)
- Code review con checklist de guardrails

**Fase 3: Deployment**
- Script de deployment ejecuta migraciones SQL en orden (v001, v002, ...)
- Rollback: script v_rollback que revierte la migración anterior
- Verificación en Azure de que los agregados funcionan con Managed Identity real

**Rollback Strategy**:
- Si algo falla en deployment: ejecutar el script de rollback correspondiente y revertir a la versión anterior
- Si la aplicación tiene un bug post-deploy: hot-fix branch desde main, cherry-pick, redeploy

## Open Questions

1. ¿Qué granularidad de auditoría es requerida? ¿Usuario, IP, timestamp, o solo usuario+timestamp?
   - **Respuesta provisional**: Usuario + timestamp. Si se requiere IP, se agrega en el handler de auditoría.

2. ¿Quién aprueba los scripts de migración SQL antes de ejecutar en producción?
   - **Respuesta provisional**: El tech lead o DBA del equipo. Se define en el CI/CD pipeline.

3. ¿Se requiere versionado de cambios en catálogos (auditoría completa del histórico)?
   - **Respuesta provisional**: Sí, auditoría de lectura en `Audit.CatalogChanges`. Si se requiere "undo" de cambios, eso es un requirement futuro (REQ_??).

4. ¿El import masivo de catálogos desde Excel (REQ_35) usa el mismo agregado o un pipeline separado?
   - **Respuesta provisional**: Mismo agregado, pero validación bulk en lugar de uno-a-uno. Se detalla en REQ_35.
