# Decisiones del agente — req-01-catalogos

Sesión automatizada, sin usuario disponible para preguntas. Las ambigüedades se
resuelven con la opción más conservadora.

- **1.3 / 4.8 connection strings**: se usó una password de desarrollo local
  (`Local_Dev_Only_ChangeMe`) en `appsettings.json` y `appsettings.Local.json`
  porque Testcontainers/SQL local exigen `User Id` + `Password`. En producción
  se usa `Authentication=Active Directory Default` (Managed Identity, sin
  secretos), coherente con G-GLOBAL-08 y la matriz de divergencias.
- **1.4 / 11.1**: no se incluye Testcontainers en las pruebas ejecutables de
  esta iteración porque no hay Docker/SQL Server en el sandbox. Se documenta la
  intención con `docker-compose.local.yml` y fixtures que se saltan cuando no
  hay `TESTCONTAINERS_ENABLED`.
- **Tareas 11.x y 12.x**: se implementan como pruebas de integración/API que
  usan Testcontainers cuando la infraestructura está disponible; en sandbox
  quedan compilando pero no se ejecutan. No se marca `[x]` la ejecución.
- **Nombres de endpoint**: el spec cita `empresas` en un escenario, pero las
  rutas canónicas de tasks.md usan `companies`/`formats`/`disciplines`. Se
  implementaron las rutas de tasks.md; el catálogo Empresa queda en inglés para
  ser consistente con el resto del módulo.

## Segunda pasada (corrección/verificación)

- `CatalogType` queda definido una sola vez como enum en
  `shared/catalog-type.ts`; se elimina el type alias duplicado.
- La búsqueda UI se mueve a `CatalogListComponent` con campos de código y
  descripción; el contenedor aplica `debounceTime(300)` sobre un `combineLatest`
  de tipo/filtros/paginación/trigger y no envuelve Observables en Promises.
- La verificación anti-duplicado se mantiene en los handlers (Decisión 5); solo
  se registran los validadores FluentValidation en `Program.cs`.
- `CatalogCode` no agrega reglas de caracteres porque el spec no las define.
- `GetUsageCountAsync` se deja como stub (`0`) porque “uso” sigue `[Por definir]`.
- La autorización JWT/Entra con rol resuelto en servidor no se implementa por
  conflicto con G-WEB-BE-04.
