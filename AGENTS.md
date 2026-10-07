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
- Backend: `dotnet build src/apps/web/backend/Backend.slnx` y `dotnet test src/apps/web/backend/Backend.slnx`
- Frontend: `cd src/apps/web/frontend && pnpm install && pnpm build` (`ng test` usa Chrome y puede no correr en el sandbox).

## Trabajar en este entorno (sandbox sin base de datos ni Docker)
- Aquí NO hay SQL Server ni se ejecutan migraciones: solo se escriben scripts, código y tests. No intentes levantar contenedores ni conectarte a una BD.
- El binario `bit` es un **stub** con soporte parcial. Pasa siempre el nombre del change (`req-01-catalogos`). `bit list`, `bit validate` y `bit doctor` no existen: si fallan, sigue y deja una línea en `docs/metricas/stub-gaps.md` (comando, qué esperabas, qué pasó).
- No hagas preguntas al usuario: no hay nadie respondiendo. Si algo es ambiguo, elige la opción más conservadora, anótala en `docs/metricas/decisiones-agente.md` y continúa.
- Tareas de `tasks.md` que requieren ejecutar algo que no existe aquí (BD real, Azure, crear PR, App Insights, `bit validate`): déjalas SIN marcar y regístralas en `docs/metricas/stub-gaps.md`. Nunca marques `[x]` algo que no hiciste.
- Nunca imprimas ni guardes tokens o secretos. No escribas el contenido de `docs/def/requisitosnegocio/` (no existe en este repo).

## Git
- Rama de trabajo: la que indique el prompt de la sesión. Commit y `git push origin HEAD` al terminar cada sección de `tasks.md`.
- Identidad: `noreply@example.com` / `agentsky`. No uses otros correos.
