# Project scaffolded with BIT Developer Framework

## 🔒 Technology Lock — DO NOT CHANGE without explicit approval

| Component | Version | Guardrail |
|-----------|---------|-----------|
| Angular | 20 (Zoneless — sin zone.js) | .github/governance/guardrails/guardrails-02-web-frontend.md |
| TypeScript | 5.7 | .github/governance/guardrails/guardrails-02-web-frontend.md |
| Tailwind CSS | 4.1 | .github/governance/guardrails/guardrails-02-web-frontend.md |
| RxJS | 7.8.1 | .github/governance/guardrails/guardrails-02-web-frontend.md |
| .NET | 10 | .github/governance/guardrails/guardrails-03-web-backend.md |
| SQL Server | 2022 | .github/governance/guardrails/estructura-carpetas.md |
| Entity Framework Core | 10 (solo ORM de consulta — migraciones son Script-Only, ver guardrails-10-database-modeling.md) | .github/governance/guardrails/guardrails-03-web-backend.md |

## 🔒 Architecture Lock

- Web Backend: Hexagonal + CQRS + MediatR (G-WEB-BE-01)
- Web Frontend: Zoneless — `OnPush` obligatorio en todos los componentes, `signal()`/`computed()` como único disparador de change detection, sin `zone.js` (G-WEB-FE-03)
- Infrastructure: Azure PaaS — contenedores/K8s prohibidos como topología de despliegue; Docker sí para dev/test local (Testcontainers, G-TEST-06)
- Layers aisladas: Presentation→Application→Domain←Infrastructure
- Migraciones de base de datos: scripts SQL versionados (Script-Only), NO migraciones de EF Core (G-DB-02, ver guardrails-10-database-modeling.md)

## 🔒 Naming Lock

- Files: kebab-case | Classes: PascalCase | Constants: CONSTANT_CASE
- Branches: feature|bugfix from develop, hotfix from main — {tipo}/desc-corta-kebab-case
- Commits: Conventional Commits (feat:, fix:, docs:, refactor:, test:, chore:)

## 🔒 Infrastructure Lock (Azure Only)

- API Management → App Service → Service Bus → Azure SQL → Redis → Key Vault → Entra ID
- ⏸️ Terraform para IaC (no Bicep, no ARM, no Pulumi) — diferido, no requerido por ahora — guardrails-09-devops-cicd.md
- NO contenedores/Kubernetes como topología de despliegue (Container Apps, AKS, App Service for Containers)
- Docker SÍ en la máquina del dev y en CI: dependencias locales, Testcontainers (G-TEST-06), Azurite

## 🔒 Testing & CI/CD Lock

- Pirámide de testing + cobertura mínima enforced en CI — guardrails-11-testing.md
- Pipeline (build/test/plan/apply/deploy) y estrategia de rollback — guardrails-09-devops-cicd.md

## 📚 Guardrails incluidos en este proyecto

- `.github/governance/guardrails/guardrails-01-global.md` — Naming, security, dependencias, métricas, desarrollo local emulado y paridad con Azure (G-GLOBAL-08)
- `.github/governance/guardrails/guardrails-09-devops-cicd.md` — Terraform (IaC, ⏸️ diferido), pipeline CI/CD, rollback
- `.github/governance/guardrails/guardrails-11-testing.md` — Pirámide de testing, cobertura mínima, exit criteria
- `.github/governance/guardrails/guardrails-02-web-frontend.md` — Angular 20 (zoneless, OnPush, signals)
- `.github/governance/guardrails/guardrails-03-web-backend.md` — .NET 10 backend: Hexagonal + CQRS + MediatR
- `.github/governance/guardrails/guardrails-10-database-modeling.md` — Modelado ER y migraciones (Script-Only)

## 🔒 Always read guardrails first

Before accepting changes to technology, architecture, naming, infrastructure, testing, or CI/CD:
1. Read the relevant guardrail in .github/governance/guardrails/
2. Identify the specific code (e.g., G-WEB-BE-03)
3. BLOCK if violation — ask for explicit approval
