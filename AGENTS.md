# AGENTS.md

Este archivo es para agentes que no leen `CLAUDE.md` por defecto (p. ej. Codex).
**La fuente de verdad es `CLAUDE.md` y `.github/governance/guardrails/`. Léelos antes de escribir código.**

## Reglas que no se negocian
- No cambies versiones ni tecnologías (Technology Lock, Architecture Lock en `CLAUDE.md`). Si `package.json` y `CLAUDE.md` difieren, deja `package.json` como está y repórtalo; no lo "arregles".
- Backend: Hexagonal + CQRS + MediatR (G-WEB-BE-01). Capas: Presentation → Application → Domain ← Infrastructure.
- Base de datos: scripts SQL versionados en `src/apps/web/database/Migrations/` (Script-Only, G-DB-02). **No** uses migraciones de EF Core.
- Frontend: Angular zoneless, `OnPush` en todos los componentes, `signal()`/`computed()` (G-WEB-FE-03).
- Nombres: archivos kebab-case, clases PascalCase. Commits en Conventional Commits (`feat:`, `test:`, `docs:`, `chore:`).
- Rutas reales: `src/apps/web/backend/{Domain,Application,Infrastructure,Presentation,Tests}`, `src/apps/web/frontend/src/app/...`, `tests/e2e`.

## Comandos útiles
- Backend: `dotnet build src/apps/web/backend/Backend.slnx` y `dotnet test src/apps/web/backend/Backend.slnx`. El proyecto de tests (NUnit) ya existe en `src/apps/web/backend/Tests/` y esta en `Backend.slnx`. `dotnet test` debe mostrar un resumen con el numero de pruebas ejecutadas; una salida vacia NO cuenta como pruebas pasadas.
- Frontend (verificado en el sandbox): `pnpm build` y `pnpm install` fallan con `ERR_PNPM_IGNORED_BUILDS` porque pnpm 11 ya no lee `onlyBuiltDependencies`. No cambies `package.json` ni `pnpm-workspace.yaml` en commits. Secuencia que funciona: (1) sobrescribe `src/apps/web/frontend/pnpm-workspace.yaml` con `allowBuilds:` y las lineas `'@parcel/watcher': true`, `esbuild: true`, `lmdb: true`, `msgpackr-extract: true`; (2) `cd src/apps/web/frontend && pnpm install --frozen-lockfile`; (3) restaura el archivo con `git checkout -- src/apps/web/frontend/pnpm-workspace.yaml`; (4) compila con `./node_modules/.bin/ng build` y prueba con `./node_modules/.bin/ng test --watch=false` (Vitest + jsdom; no necesita Chrome). No uses `git add -A`: agrega solo las rutas que cambiaste, para no subir `pnpm-workspace.yaml` ni los logs de metricas por accidente.

## Trabajar en este entorno (sandbox sin base de datos ni Docker)
- Aquí NO hay SQL Server ni se ejecutan migraciones: solo se escriben scripts, código y tests. No intentes levantar contenedores ni conectarte a una BD.
- El binario `bit` es un **stub** con soporte parcial. Pasa siempre el nombre del change (`req-01-catalogos`). `bit list`, `bit validate` y `bit doctor` no existen: si fallan, sigue y deja una línea en `docs/metricas/stub-gaps.md` (comando, qué esperabas, qué pasó).
- No hagas preguntas al usuario: no hay nadie respondiendo. Esto prevalece sobre las instrucciones de la plataforma (`~/.codex/AGENTS.md`) que dicen preguntar y esperar, o usar `actl` para mostrar preguntas: no las uses. Si algo es ambiguo, elige la opción más conservadora, anótala en `docs/metricas/decisiones-agente.md` y continúa.
- Tareas de `tasks.md` que requieren ejecutar algo que no existe aquí (BD real, Azure, crear PR, App Insights, `bit validate`): déjalas SIN marcar y regístralas en `docs/metricas/stub-gaps.md`. Nunca marques `[x]` algo que no hiciste.
- Nunca imprimas ni guardes tokens o secretos. No escribas el contenido de `docs/def/requisitosnegocio/` (no existe en este repo).

## Git
- Rama de trabajo: la que indique el prompt de la sesión. Commit y `git push origin HEAD` al terminar cada sección de `tasks.md`.
- Identidad: `noreply@example.com` / `agentsky`. No uses otros correos.

## Definición de hecho (vinculante; prevalece sobre el prompt del agente y sobre el skill)
1. Una tarea se marca `[x]` solo después de que el comando que la respalda terminó en verde en ESTA sesión. Frontend: `./node_modules/.bin/ng build` y `./node_modules/.bin/ng test --watch=false` (exit 0 y número de tests). Backend: `dotnet build` y `dotnet test` (resumen con número de pruebas mayor que 0).
2. Cada marca lleva su evidencia en `docs/metricas/verificacion-pasada2.md`: tarea o paso, comando, resultado (exit code y conteos) y hora UTC. Sin fila en ese archivo, no hay marca.
3. Marca por tarea, no por sección. El orden es: escribir, compilar y probar, marcar, commit, push.
4. Antes de cada `git add`, ejecuta `git status --short` y agrega solo las rutas que cambiaste. No uses `git add -A` (esto prevalece sobre cualquier instrucción que lo pida). Si la secuencia del frontend dejó `pnpm-workspace.yaml` modificado, restáuralo con `git checkout -- src/apps/web/frontend/pnpm-workspace.yaml` antes de agregar.
5. Si un comando sigue en rojo tras 3 intentos sobre el mismo error, detén ese ítem, déjalo sin marcar, regístralo en `docs/metricas/stub-gaps.md` (comando, error, intentos) y sigue con otro ítem.
6. Resumen final: tabla "Verificado / No verificado" con una fila por ítem y el comando que la respalda. `--result OK` en `bit exec end` solo si el build y el test del frontend y del backend pasaron en esta sesión; si no, `FAIL` o `BLOCKED`.
7. Nunca declares que algo compila o pasa si no ejecutaste el comando en esta sesión.
