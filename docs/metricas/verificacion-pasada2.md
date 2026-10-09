# Verificación pasada 2 — req-01-catalogos

Sesión iniciada 2026-10-09T13:53:04Z. Rama `feature/agentsky-apply-req-01-catalogos-pass2`.

## Versiones

| Herramienta | Comando | Resultado | Exit |
|---|---|---|---|
| Node | `node -v` | v24.21.0 | 0 |
| .NET | `dotnet --version` | 10.0.401 | 0 |
| pnpm (raíz) | `pnpm -v` | 11.22.0 | 0 |
| pnpm (frontend) | `cd src/apps/web/frontend && pnpm -v` | 11.22.0 (warning `pnpm.onlyBuiltDependencies` ignorado) | 0 |

## Evidencia de comandos

| Tarea o paso | Comando | Resultado (exit code y conteos) | Hora UTC |
|---|---|---|---|
| 0.d versiones | `node -v; dotnet --version; pnpm -v; cd src/apps/web/frontend && pnpm -v` | exit 0; versiones arriba | 2026-10-09T13:53:04Z |
| 1.a frontend install | `cat > pnpm-workspace.yaml (allowBuilds); cd src/apps/web/frontend && pnpm install --frozen-lockfile` | exit 0; 467 packages installed; warning pnpm.onlyBuiltDependencies ignorado | 2026-10-09T13:53:53Z |
| 1.a frontend build (pre-corrección) | `./node_modules/.bin/ng build` | exit 1; 9 errores TS (CatalogType usado como valor, subscribe/Promise mal tipado, items sobre Observable, resource sin return) | 2026-10-09T13:53:53Z |
| 1.a frontend test (pre-corrección) | `./node_modules/.bin/ng test --watch=false` | exit 1; 10 errores de compilación; 0 tests ejecutados | 2026-10-09T13:54:02Z |
| 1.b backend build | `dotnet build src/apps/web/backend/Backend.slnx` | exit 0; 0 errores; 5 warnings (NU1903 x3/4, CS0618 Testcontainers) | 2026-10-09T13:54:18Z |
| 1.b backend test | `dotnet test src/apps/web/backend/Backend.slnx --no-build` | exit 0; Passed 22, Failed 0, Skipped 0, Total 22 | 2026-10-09T13:54:21Z |
| 3.A frontend build (corregido) | `cd src/apps/web/frontend && ./node_modules/.bin/ng build` | exit 0; build Angular OK, 0 errores | 2026-10-09T14:01:20Z |
| 3.B frontend test | `cd src/apps/web/frontend && ./node_modules/.bin/ng test --watch=false` | exit 0; 5 test files, 19 tests passed | 2026-10-09T14:03:17Z |
| 3.C frontend build | `cd src/apps/web/frontend && ./node_modules/.bin/ng build` | exit 0; 0 errores | 2026-10-09T14:04:22Z |
| 3.C frontend test | `cd src/apps/web/frontend && ./node_modules/.bin/ng test --watch=false` | exit 0; 5 files, 19 tests | 2026-10-09T14:04:25Z |
| 3.D backend build | `dotnet build src/apps/web/backend/Backend.slnx` | exit 0; 0 errores; 5 warnings | 2026-10-09T14:05:29Z |
| 3.D backend test | `dotnet test src/apps/web/backend/Backend.slnx --no-build` | exit 0; Passed 22, Failed 0, Skipped 0, Total 22 | 2026-10-09T14:05:30Z |
