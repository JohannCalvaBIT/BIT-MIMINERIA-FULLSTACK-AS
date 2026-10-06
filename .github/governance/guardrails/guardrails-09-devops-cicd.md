# GUARDRAILS - DevOps & CI/CD Pipeline

**Rol**: DevOps Engineer / Tech Lead (todos los stacks)
**Aplicable a**: WEB + INTEGRATION + MOBILE
**Última actualización**: 2026-09-04

---

## G-CICD-01: Infrastructure as Code (Terraform-Only, No Exceptions)

> ⏸️ **Diferido (2026-09-16)**: por decisión explícita del owner, la implementación de IaC no es requisito bloqueante por ahora — no se generan ni exigen `.tf` en scaffolding ni en `/opsx:apply`. Cuando el proyecto retome infraestructura, Terraform sigue siendo la única herramienta permitida (Bicep/ARM/Pulumi siguen prohibidos); las reglas de esta sección aplican en ese momento.

**IaC Provider**
- ✅ Terraform 1.9+ para TODA la infraestructura Azure
- ✅ Estado remoto (Azure Storage backend) con locking
- ✅ Variables por entorno: `terraform.tfvars.<env>` (nunca committeadas con valores reales)
- ✅ `plan` revisado en PR antes de cualquier `apply`
- ❌ Bicep
- ❌ ARM templates
- ❌ Pulumi
- ❌ Cambios manuales en Azure Portal como mecanismo principal (solo para diagnóstico de emergencia, con ticket de seguimiento para reconciliar el estado en Terraform)

**Ejecución**
- ✅ `terraform plan` y `terraform apply` **solo** desde pipelines controlados (GitHub Actions)
- ✅ Aprobación manual requerida antes de `apply` en producción
- ❌ `terraform apply` local contra el estado de producción

```hcl
# ✅ CORRECT — backend remoto con locking
terraform {
  backend "azurerm" {
    resource_group_name  = "rg-tfstate"
    storage_account_name = "sttfstateprod"
    container_name        = "tfstate"
    key                    = "web.terraform.tfstate"
  }
}
```

---

## G-CICD-02: Pipeline Stages (Mandatory, No Skipping)

Toda pipeline de CI/CD debe implementar, en este orden, las siguientes etapas:

```
1. build              → compilar (dotnet build / ng build / flutter build)
2. test               → unit + integration tests
3. security-scan      → escaneo de secretos + dependencias vulnerables
4. deploy-staging     → despliegue automático a staging tras pasar 1-3
5. deploy-production  → requiere aprobación manual explícita
```

- ✅ Cada etapa bloquea la siguiente si falla (fail-fast)
- ✅ `test` incluye gate de cobertura mínimo del **70%** en los cuatro stacks (ver `G-TEST-04`):
  - **Backend / Integración (.NET)** vía `coverlet.msbuild`:
    ```bash
    dotnet test src/apps/web/backend/ --no-build \
      /p:CollectCoverage=true \
      /p:CoverletOutputFormat=cobertura \
      /p:Threshold=70 \
      /p:ThresholdType=line \
      /p:ThresholdStat=total \
      /p:Exclude="[*]Program"
    ```
    Requiere que el proyecto de tests referencie el paquete NuGet `coverlet.msbuild` (incluido por defecto al crear el proyecto con `dotnet new nunit`/`dotnet new xunit`). Sin exclusión de `*.Migrations.*`/`*ModelSnapshot`: las migraciones son scripts SQL (Script-Only, ver G-DB-02), no hay clases de EF Core Migrations que excluir.
  - **Frontend (Angular)** vía `karma.conf.js` → `coverageReporter.check.global` (solo presente si el frontend fue bootstrapeado por el paso automático de `bit init`; sin él, la cobertura se mide pero no bloquea).
  - **Mobile (Flutter)** vía `flutter test --coverage` + un script que suma `LF`/`LH` de `lcov.info` y calcula el % de líneas cubiertas.
- ✅ `test` es real, no solo "las herramientas están instaladas": el CI generado por `bit init` corre `build-test-frontend` (`pnpm lint` + test con cobertura + `pnpm build`), `build-test-backend` (con gate de cobertura), `build-test-integracion` (con gate de cobertura), y `build-test-mobile` (`flutter analyze` + test con gate de cobertura) — cada uno solo si el stack correspondiente fue elegido.
- ✅ `security-scan` audita dependencias: `pnpm audit --audit-level=high` (frontend) y `dotnet list package --vulnerable --include-transitive` (backend/integración, revisando el texto de salida porque el comando no falla por sí solo) — bloquea si encuentra severidad alta/crítica
- ✅ `bit init` (stacks `web`/`all`) scaffoldea `tests/e2e/` con un smoke test real de Playwright (`tests/e2e/tests/smoke.spec.ts`) y su propio `package.json`/`playwright.config.ts`. El job `e2e` del CI generado instala dependencias + navegadores y corre `pnpm test`; Playwright arranca `pnpm start` del frontend por su cuenta (config `webServer`) y espera a que responda antes de correr los tests — no requiere orquestar el servidor a mano en el workflow.
- ✅ `deploy-production` requiere al menos 1 aprobador distinto del autor del cambio
- ❌ Saltar `test` o `security-scan` "porque hay prisa"
- ❌ Deploy directo a producción sin pasar por staging

```yaml
# ✅ CORRECT — GitHub Actions, gate de aprobación en production
jobs:
  deploy-production:
    needs: [build, test, security-scan, deploy-staging]
    environment:
      name: production   # requiere reviewers configurados en Settings > Environments
    runs-on: ubuntu-latest
    steps:
      - run: echo "Deploying to production"
```

---

## G-CICD-03: Deployment Gates, Health Checks & Rollback

**Quality Gates (bloquean el pipeline)**
- ✅ Build exitoso
- ✅ Tests unitarios + integración en verde
- ✅ Cobertura mínima respetada (`G-GLOBAL-05`: 70% frontend, 75% backend)
- ✅ Escaneo de seguridad sin severidad crítica
- ✅ `bit verify` en verde (o solo `WARN`, nunca `FAIL`)

**Post-Deploy**
- ✅ Health check obligatorio inmediatamente después de cada deploy: `GET /health` y `GET /health/live`
- ✅ Si el health check falla dentro de la ventana de smoke-test (ej. 5 min), rollback automático
- ✅ Plan de rollback documentado y probado por servicio (no improvisado en el momento del incidente)
- ❌ Marcar un deploy como exitoso sin validar el health check
- ❌ Rollback manual sin un procedimiento documentado

**Rollback Strategy por Componente**

| Componente | Estrategia |
|-----------|-----------|
| Web/Integration (.NET) | Redeploy del slot anterior (blue-green) o versión previa del artifact |
| Base de datos | Solo `Down()`/rollback script si es reversible; si no, roll-forward con fix |
| Mobile (Flutter) | No hay rollback de app instalada — usar feature flags para desactivar funcionalidad rota |

---

## G-CICD-04: Secrets en CI/CD

- ✅ Secrets inyectados vía Azure Key Vault + Managed Identity, nunca como GitHub Secrets planos para producción
- ✅ Variables sensibles enmascaradas en logs (`::add-mask::` en GitHub Actions)
- ✅ Rotación de secretos cada 90 días (alineado con `G-GLOBAL-03`)
- ❌ Secrets impresos en logs de pipeline (ni siquiera en debug mode)
- ❌ Secrets en archivos `.yml` de workflow versionados

---

## G-CICD-05: Ambientes y Promoción

- ✅ Progresión estricta: `dev → staging → production`
- ✅ Paridad de configuración entre staging y production (mismo Terraform, distintas `tfvars`)
- ✅ Feature flags para activar funcionalidad incompleta sin bloquear el release
- ❌ Ambientes staging con infraestructura significativamente distinta a producción (invalida las pruebas)

---

## Checklist Pre-Merge / Pre-Deploy

- [ ] Pipeline con las 5 etapas completas (build, test, security-scan, deploy-staging, deploy-production)
- [ ] `terraform plan` revisado y adjunto al PR (si el cambio toca infraestructura)
- [ ] Sin secretos en el diff ni en el workflow
- [ ] Health checks configurados y validados en staging
- [ ] Plan de rollback documentado para el servicio afectado
- [ ] Aprobación manual registrada antes de `deploy-production`

Si falla cualquier punto de este checklist, BLOQUEAR el deploy y pedir ajustes.
