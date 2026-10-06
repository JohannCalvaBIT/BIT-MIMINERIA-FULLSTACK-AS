# BIT-MIMINERIA-FULLSTACK

**Contexto:** (pendiente)
**Stack:** Angular 20 + .NET 10 + Entity Framework Core 10 + Azure SQL
**Arquitectura:** Hexagonal Architecture + CQRS

## Estructura

```
BIT-MIMINERIA-FULLSTACK/
  docs/                        Documentation
  src/apps/web/
    frontend/                  Angular 20 + Tailwind
    backend/                   .NET 10 (Hexagonal + CQRS)
    database/                  SQL Server + EF Core migrations
  tests/                       E2E + functional
  infrastructure/terraform/    IaC (Azure) — ⏸️ diferido, no scaffoldeado por ahora
  openspec/specs/              AI agent context
  .github/                     Governance + CI/CD
```

## Comandos

```bash
bit verify                                                       # Verificar código antes de merge
bit metrics "<rol>" "<actividad>" <estimado> <real> --guardrail <codigo>  # Registrar actividad
bit propose "<descripcion>"                                      # Proponer un cambio (OpenSpec)
```

## Desarrollo

```bash
# Frontend
cd src/apps/web/frontend && pnpm install && pnpm start

# Backend
cd src/apps/web/backend && dotnet restore && dotnet run
```

## Gobierno

- Guardrails: `.github/governance/guardrails/`
- Verificación: `bit verify` (ver `docs/verify.md` en el framework)
- Métricas: `docs/metricas/BIT-MIMINERIA-FULLSTACK_metricas.csv`, `docs/metricas/guardrail_activity_log.csv`
- Especificaciones para el agente IA: `openspec/specs/`
- Reglas congeladas: `CLAUDE.md`, `.github/copilot-instructions.md`
