# BIT Developer Framework — Copilot Instructions

## 🔒 Technology Stack — LOCKED

| Component | Version | File |
|-----------|---------|------|
| Angular | 20.0.0 (Zoneless — sin zone.js) | .github/governance/guardrails/guardrails-02-web-frontend.md |
| TypeScript | 5.7 | .github/governance/guardrails/guardrails-02-web-frontend.md |
| Tailwind CSS | 4.1 | .github/governance/guardrails/guardrails-02-web-frontend.md |
| RxJS | 7.8.1 | .github/governance/guardrails/guardrails-02-web-frontend.md |
| .NET | 10.0 | .github/governance/guardrails/guardrails-03-web-backend.md |
| Entity Framework Core | 10.0 (solo ORM de consulta — migraciones son Script-Only, ver guardrails-10-database-modeling.md) | .github/governance/guardrails/guardrails-03-web-backend.md |
| SQL Server | 2022 | .github/governance/guardrails/estructura-carpetas.md |
| Terraform | 1.9+ (⏸️ diferido, no requerido por ahora) | .github/governance/guardrails/estructura-carpetas.md |

Cualquier cambio requiere aprobacion explicita.

## 🔒 Architecture — LOCKED

- Web Backend: Hexagonal + CQRS + MediatR
- Web Frontend: Zoneless — `OnPush` obligatorio, `signal()`/`computed()` como único disparador de change detection, sin `zone.js`
- Infrastructure: Azure PaaS — contenedores/K8s prohibidos como topología de despliegue; Docker sí para dev/test local (Testcontainers, G-TEST-06)
- Migraciones de BD: scripts SQL versionados (Script-Only), NO EF Core Migrations (ver guardrails-10-database-modeling.md)

## 🔒 Naming — LOCKED

- Files: kebab-case | Classes: PascalCase | Constants: CONSTANT_CASE
- Branches: feature/bugfix from develop, hotfix from main — {tipo}/desc-corta-kebab-case
- Commits: Conventional Commits

## 🔒 Infrastructure — LOCKED (Azure Only)

API Management → App Service / Service Bus / Azure SQL / Redis / Key Vault / Entra ID / App Insights
⏸️ Terraform solo — NO Bicep, NO ARM, NO Pulumi — diferido, no requerido por ahora (guardrails-09-devops-cicd.md)
NO contenedores/Kubernetes como topología de despliegue (Container Apps, AKS, App Service for Containers)
Docker SÍ en la máquina del dev y en CI: dependencias locales, Testcontainers (G-TEST-06), Azurite

## 🔒 Testing & CI/CD — LOCKED

- Pirámide de testing + cobertura mínima enforced en CI (guardrails-11-testing.md)
- Pipeline y estrategia de rollback (guardrails-09-devops-cicd.md)

## 🔒 Metrics — OBLIGATORIO

- docs/metricas/<project>_metricas.csv (G-GLOBAL-06)
- docs/metricas/guardrail_activity_log.csv (G-GLOBAL-07)

## 📚 Guardrails incluidos en este proyecto

- `.github/governance/guardrails/guardrails-01-global.md` — Naming, security, dependencias, métricas, desarrollo local emulado y paridad con Azure (G-GLOBAL-08)
- `.github/governance/guardrails/guardrails-09-devops-cicd.md` — Terraform (IaC, ⏸️ diferido), pipeline CI/CD, rollback
- `.github/governance/guardrails/guardrails-11-testing.md` — Pirámide de testing, cobertura mínima, exit criteria
- `.github/governance/guardrails/guardrails-02-web-frontend.md` — Angular 20 (zoneless, OnPush, signals)
- `.github/governance/guardrails/guardrails-03-web-backend.md` — .NET 10 backend: Hexagonal + CQRS + MediatR
- `.github/governance/guardrails/guardrails-10-database-modeling.md` — Modelado ER y migraciones (Script-Only)

## Regla de oro

Antes de cambiar tecnologia/arquitectura/infraestructura/testing/CI-CD: leer guardrail, identificar codigo, BLOQUEAR, pedir confirmacion.
