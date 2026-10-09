# Stub gaps — comandos y tareas no ejecutables en el sandbox

## Estado real de `bit exec`

- `bit exec start` y `bit exec end` están soportados por el stub y **sí escriben**
  su fila en `docs/metricas/execution_log.ndjson`. La afirmación previa de que no
  escribían era incorrecta; se verificó el registro `start` de esta segunda pasada.
- `bit status --change "req-01-catalogos" --json` y
  `bit instructions apply --change "req-01-catalogos" --json` existen y devuelven
  JSON inferido por el stub. Los campos no nombrados por el skill no provienen del
  `bit` real.
- `bit list`, `bit validate` y `bit doctor` no existen en el stub. Por eso la
  validación automática de OpenSpec contra guardrails queda sin ejecutar.

## Tareas sin respaldo en esta pasada

- **2.4 (`CatalogCode` caracteres permitidos)**: el spec solo define límite de
  longitud; no define el conjunto de caracteres permitidos. No se inventa regla,
  por lo que queda sin marcar.
- **3.10 (anti-duplicado en FluentValidation)**: los validadores de comandos
  existen y ahora se registran en `Program.cs`, pero la verificación anti-duplicado
  contra repositorio vive en los handlers por la Decisión 5 del design. Moverla al
  validador cambiaría el diseño; queda sin marcar.
- **4.2 / 4.3 / 4.4 (`GetUsageCountAsync` real)**: qué cuenta como “uso” sigue
  `[Por definir]`; los repositorios devuelven temporalmente 0. Quedan sin marcar.
- **8.5 / 8.6 (indicador “En uso” y bloqueo)**: dependen de `GetUsageCountAsync`
  real. Quedan sin marcar.
- **9.3 / autenticación JWT/Entra y rol resuelto en servidor**: la spec choca con
  G-WEB-BE-04 (el backend no emite ni resuelve tokens propios). No se implementa.
- **Secciones 11 y 12 (integración/endpoints)**: requieren SQL Server/Testcontainers
  y Docker, no disponibles. El código compila pero no se ejecuta.
- **13.5 / 13.6 (e2e)**: requieren backend + SQL Server reales. Quedan sin marcar.
- **14.3 (`bit validate`)**: el stub no implementa el comando.
- **14.4 (crear PR)**: no hay capacidad de abrir PRs desde el sandbox; se empuja
  `git push origin HEAD`.
- **15.1 (smoke Azure)**: no hay Azure ni Managed Identity.
- **15.2 (`run-migrations.ps1` en staging)**: no hay SQL Server ni staging.
- **15.3**: duplica la tarea 6.4 (`v001_rollback.sql`, ya creada). No se marca una
  segunda vez.
- **15.4 (App Insights)**: no hay App Insights disponible.
- **15.5**: se cubre con `bit exec end --log-activity` y
  `docs/metricas/guardrail_activity_log.csv`.

## Decisiones humanas o de infraestructura

- Contraseña de desarrollo en `appsettings*.json`: decisión de seguridad ya
  registrada en `docs/metricas/decisiones-agente.md`.
- `sqlcmd -C` y la advertencia `NU1903` de `Microsoft.OpenApi`: son decisiones de
  versiones/seguridad; no se cambian paquetes ni versiones.
- La primera pasada **no ejecutó** `ng build` ni `ng test`. Esta segunda pasada
  ejecutó ambos y dejó la evidencia en `docs/metricas/verificacion-pasada2.md`.
