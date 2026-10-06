# GUARDRAILS - Frontend Developer (Angular 20)

**Rol**: Frontend Developer  
**Stack**: Angular 20.0.0 (Zoneless — sin `zone.js`) + TypeScript 5.8 + Tailwind CSS 4.1 + RxJS 7.8.2  
**Última actualización**: 2026-09-23

> **Por qué TypeScript 5.8 y RxJS 7.8.2, no versiones más nuevas**: son las versiones exactas que fija Angular 20 como peer dependency (`typescript: ">=5.8.0 <6.0.0"`, `rxjs: "^6.5.3 || ^7.4.0"`). TypeScript 6.0 y RxJS 9 (quiebre arquitectónico basado en `Observable` nativo del browser, y todavía en beta) están fuera de ese rango — instalarlos rompe el build de Angular 20. Subir estas dos versiones requiere subir Angular primero, y eso es un cambio de Technology Lock aparte, no un simple bump de patch.

---

## G-WEB-FE-00: Standalone Components & Dependency Injection

- ✅ Standalone components únicamente — sin `NgModule` (Angular 20 no lo requiere; `ng new` ya genera standalone por default)
- ✅ Bootstrap vía `bootstrapApplication()` + `provideRouter()`/`provideHttpClient()` en `app.config.ts`
- ✅ `inject()` como patrón preferido sobre constructor injection — necesario de todas formas para guards funcionales (`CanActivateFn`, ver G-WEB-FE-05) y resolvers funcionales
- ❌ `NgModule` para features nuevas (legacy — solo aceptable en código heredado no migrado)

---

## G-WEB-FE-01: Component Composition Rules

### Component Isolation (Non-negotiable)
- ✅ Components communicate ONLY via `input()` / `output()` (signal-based, ver G-WEB-FE-03 — NO `@Input`/`@Output` decorador clásico en componentes nuevos)
- ❌ NO direct service calls in presentational components
- ✅ Smart components (containers) wrap dumb components
- ✅ Smart components inject services, pass data down
- ✅ Data flows one-way: parent → child

### Component Lifecycle
```typescript
// ✅ CORRECT
export class ProductListComponent implements OnInit, OnDestroy {
  products$ = this.service.getProducts();
  
  ngOnInit() { /* initial setup */ }
  ngOnDestroy() { /* cleanup, unsubscribe */ }
}

// ❌ WRONG
export class ProductListComponent {
  products: Product[];
  
  constructor(private service: ProductService) {
    this.service.getProducts().subscribe(p => this.products = p); // Memory leak!
  }
}
```

---

## G-WEB-FE-02: State Management Rules

### Global State (Signals por defecto — NgRx solo si el estado compartido lo justifica)
- ✅ Zoneless requiere que el estado que afecta la vista viva en un `signal()`/`computed()`; un `BehaviorSubject` o una variable plana NO dispara re-render por sí sola
- ✅ Auth state: GLOBAL (shared across app)
- ✅ User preferences: GLOBAL (theme, language)
- ✅ Feature state: LOCAL (orders, products specific)
- ❌ Component-level state NEVER in global store
- ❌ Multiple sources of truth (pick ONE)

### Observable Patterns
- ✅ Use `async` pipe to auto-unsubscribe
- ✅ Manual `subscribe()` only if business logic required
- ✅ Unsubscribe in `ngOnDestroy()` if subscribed manually
- ✅ `toSignal()` (`@angular/core/rxjs-interop`) cuando el dato de un Observable de servicio necesita combinarse con un `computed()` local — el puente oficial entre RxJS y Signals, en vez de mezclar `subscribe()` manual con asignación a una propiedad plana
- ❌ NO nested `subscribe()` (use `switchMap`, `mergeMap`)

---

## G-WEB-FE-03: Template Best Practices

### Change Detection (Zoneless — `provideZonelessChangeDetection()`, sin `zone.js`)
- ✅ `zone.js` NO está en `angular.json` (`polyfills`) ni en `package.json` — el proyecto no lo instala
- ✅ `provideZonelessChangeDetection()` en `app.config.ts` (`bootstrapApplication`)
- ✅ `ChangeDetectionStrategy.OnPush` en TODOS los componentes (sin zone.js no hay un "Default" que se re-chequee solo)
- ✅ Estado reactivo en `signal()` / `computed()` — son lo único que dispara re-render automáticamente sin zone.js
- ✅ `input()`/`output()` (signal inputs/outputs) en vez de `@Input()`/`@Output()` decorador clásico para componentes nuevos
- ⚠️ `markForCheck()` / `ChangeDetectorRef.detectChanges()`: solo como último recurso al interoperar con una librería de terceros que mute estado fuera de un signal (no como patrón normal — si lo necesitas seguido, ese estado debería ser un signal)
- ❌ Confiar en que un callback async (timeout, evento DOM, promesa) dispare change detection "solo" — sin zone.js eso no pasa a menos que el dato que cambió sea un signal

### Binding
- ✅ Property binding: `[property]="value"`
- ✅ Event binding: `(click)="method()"`
- ✅ Two-way (forms): `[(ngModel)]` or `FormControl`
- ❌ Property access: `{{ obj.prop }}` for complex logic (use pipe)
- ❌ Method calls in templates: `{{ getData() }}` (called on every tick)

### Control Flow (bloques nativos — reemplazan las structural directives en código nuevo)
- ✅ `@if` / `@else if` / `@else`: show/hide conditionally — reemplaza `*ngIf`
- ✅ `@for (item of items; track item.id)`: `track` es obligatorio en la sintaxis (no se puede omitir como sí pasaba con `trackBy` en `*ngFor`) — reemplaza `*ngFor`
- ✅ `@switch` / `@case` / `@default`: multiple conditionals — reemplaza `*ngSwitch`
- ✅ Ventajas sobre las structural directives: no requieren `CommonModule`, mejor narrowing de tipos dentro del bloque, mejor rendimiento (compilan a instrucciones nativas, no a directivas)
- ❌ `*ngIf`/`*ngFor`/`*ngSwitch` en código nuevo — solo aceptable en código heredado no migrado
- ❌ NO nested `@if` (refactor to service/pipe)

---

## G-WEB-FE-04: Form Handling

### Reactive Forms (Recommended)
- ✅ `FormBuilder`, `FormGroup`, `FormControl`, `FormArray`
- ✅ Custom validators: `Validators.required`, `Validators.pattern`
- ✅ Async validators: `email.asyncvalidation.ts`
- ✅ `valueChanges` observable for reactive behavior
- ✅ `markAllAsTouched()` on submit to show errors

### Validation
- ✅ Frontend: UX feedback (fast)
- ✅ Backend: security enforcement (definitive)
- ❌ Frontend validation ALONE (user can bypass)
- ❌ Backend validation HIDDEN (poor UX)

---

## G-WEB-FE-05: Routing Security

### Route Guards
- ✅ `CanActivateFn`: protect routes (auth required)
- ✅ `CanActivateChildFn`: protect child routes
- ✅ `CanDeactivateFn`: prevent accidental navigation (unsaved changes)
- ✅ Check permissions from `AuthService`

### Lazy Loading
- ✅ Lazy-load feature modules: faster initial load
- ✅ Configure in router: `loadComponent`, `loadChildren`
- ❌ NO eager-loading of large features (performance)
