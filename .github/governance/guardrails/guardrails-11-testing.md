# GUARDRAILS - Testing Strategy

**Rol**: Todos los desarrolladores (todos los stacks) + QA
**Aplicable a**: WEB + INTEGRATION + MOBILE
**Última actualización**: 2026-09-04

---

## G-TEST-01: Testing Pyramid (Mandatory Order)

```
        ▲
       / \      E2E (Playwright)          — flujos críticos de negocio
      /___\     Integration                — servicio + dependencias reales
     /_____\    Unit                        — la base, la mayoría de los tests
```

- ✅ Unit tests primero: cada componente/servicio/caso de uso tiene su propia suite
- ✅ Integration tests: contra dependencias reales cuando sea viable (SQL Server real, no mocks de repositorio en la capa de infraestructura — ver `G-WEB-BE-01`)
- ✅ E2E con Playwright: solo para flujos críticos de negocio, no para cada pantalla
- ❌ Invertir la pirámide (muchos E2E, pocos unit) — lento y frágil
- ❌ Mockear la base de datos en tests de integración (un mock desincronizado del schema real puede dejar pasar una migración rota que el mock nunca detecta)

---

## G-TEST-02: Criterio de Salida (Exit Criteria)

**No se considera terminado un componente/API/microservicio/pantalla sin unit tests en verde.**

- ✅ Todo entregable de desarrollo requiere pruebas unitarias del desarrollador que lo escribió — no es responsabilidad exclusiva de QA
- ✅ Si no hay pruebas unitarias, el cambio **no avanza** a integración ni E2E
- ✅ Evidencia de resultados adjunta al PR para cambios críticos (auth, pagos, datos de negocio sensibles)
- ❌ Mergear con `pnpm test` / `dotnet test` / `flutter test` en rojo, "para arreglarlo después"

---

## G-TEST-03: Casos Mínimos por Feature

Toda feature con lógica de negocio no trivial debe cubrir, como mínimo:

| Caso | Descripción | Ejemplo |
|------|-------------|---------|
| **Happy path** | El flujo esperado funciona | Crear orden con datos válidos → 201 Created |
| **Negativo** | Entrada inválida es rechazada | Crear orden sin `customerId` → 400 Bad Request |
| **Borde** | Límites del dominio | Cantidad = 0, string al límite de longitud, paginación en el último elemento |
| **Error** | Falla de dependencia manejada correctamente | Base de datos no disponible → 500 sin stack trace filtrado, log con correlation ID |

- ❌ Solo cubrir el happy path y llamarlo "cobertura completa"

---

## G-TEST-04: Cobertura Mínima (alineado con G-GLOBAL-05)

| Capa | Objetivo aspiracional | Gate real en CI |
|------|------------------------|------------------|
| Backend (.NET, Web) | 75% | **70%** (enforced vía `coverlet.msbuild` — ver `G-CICD-02`) |
| Frontend (Angular) | 70% | **70%** (enforced vía `karma.conf.js` `check.global`, solo si el frontend se bootstrapeó con `bit init` — ver `G-CICD-02`) |
| Mobile (Flutter) — Domain + Application | 75% | **70%** (enforced vía `flutter test --coverage` + script de umbral sobre `lcov.info` — ver `G-CICD-02`) |
| Integration (.NET) | 75% | **70%** (enforced vía `coverlet.msbuild`, mismo patrón que Backend — ver `G-CICD-02`) |

- ✅ Los cuatro stacks tienen cobertura medida y bloqueada en CI en **70%** (línea base incremental hacia el 75%/70% aspiracional de cada uno, no de un salto — ver histórico: Backend arrancó en 60%)
- ✅ Frameworks/librerías de terceros (EF Core, ASP.NET Core, Angular, Flutter SDK) **nunca** cuentan en este número — las herramientas de cobertura solo instrumentan el código propio del proyecto
- ✅ Excluidos del cálculo (.NET): `Program.cs` — código generado/boilerplate no testeable de forma significativa. No hay clases `*.Migrations.*`/`*ModelSnapshot` que excluir: las migraciones son scripts SQL (Script-Only, ver G-DB-02), no código C# compilado
- ❌ Excluir archivos del cálculo de cobertura para "maquillar" el número sin justificación documentada
- ❌ Bajar el 70% de cualquiera de los cuatro gates para pasar un PR — si un cambio legítimo no puede cumplirlo, se escribe el test faltante, no se relaja el umbral

---

## G-TEST-05: Datos de Prueba

- ✅ Datos de prueba anónimos y sintéticos — nunca PII real
- ✅ Fixtures sin secretos ni claves reales (ni siquiera de ambientes de desarrollo)
- ✅ Limpieza de datos de prueba al finalizar la suite (no dejar residuos en bases compartidas de staging)
- ❌ Copiar datos de producción a fixtures de test "porque son realistas"
- ❌ Credenciales reales en datasets de prueba (ver `G-GLOBAL-03`)

---

## G-TEST-06: Convenciones por Stack

**Frontend (Angular + Karma/Jasmine)**
- ✅ Unit test por componente y por servicio
- ✅ `TestBed` para componentes con dependencias inyectadas (con `provideZonelessChangeDetection()` — ver G-WEB-FE-03)
- ✅ E2E con Playwright para flujos de negocio (login, checkout, etc.) — scaffolding base en `tests/e2e/`, ver G-CICD-02

**Backend (.NET + NUnit)**
- ✅ Unit tests: `UnitTests/` — Domain + Application, sin base de datos real
- ✅ Integration tests: `IntegrationTests/` — contra SQL Server real (TestContainer o instancia de test dedicada)
- ✅ Un test por caso de uso/handler de Command o Query

**Mobile (Flutter)**
- ✅ Unit tests: Domain (usecases) + Application (BLoCs) ≥ 75%
- ✅ Widget tests: pantallas e interacciones clave
- ✅ Integration tests: flujos de usuario felices, base de datos de test separada de producción
- ❌ Testear detalles de implementación de un widget (testear comportamiento, no estructura interna)

**Integration Service (.NET + MassTransit)**
- ✅ Consumers probados con harness de test de MassTransit (in-memory bus)
- ✅ Idempotencia verificada explícitamente (mismo mensaje dos veces = mismo resultado)

---

## Checklist Pre-Merge

- [ ] Suite crítica en verde (`pnpm test` / `dotnet test` / `flutter test`)
- [ ] Casos mínimos cubiertos: happy path, negativo, borde, error
- [ ] Cobertura respeta el mínimo de `G-TEST-04` para la capa modificada
- [ ] Sin datos sensibles ni PII en fixtures o datos de prueba
- [ ] Evidencia de resultados adjunta si el cambio es crítico (auth, pagos, datos sensibles)

Si falla cualquier punto de este checklist, BLOQUEAR el merge y pedir ajustes.
