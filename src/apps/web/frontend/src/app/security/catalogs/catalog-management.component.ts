import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { combineLatest, debounceTime, map, Observable, switchMap } from 'rxjs';
import { CatalogListComponent } from './components/catalog-list.component';
import { CatalogSelectorComponent } from './components/catalog-selector.component';
import { CatalogFormComponent } from './components/catalog-form.component';
import { CatalogService } from './services/catalog.service';
import { CatalogItemDto } from './shared/catalog.types';
import { CatalogType } from './shared/catalog-type';

@Component({
  selector: 'app-catalog-management',
  standalone: true,
  imports: [CatalogSelectorComponent, CatalogListComponent, CatalogFormComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <h1>Catálogos</h1>
    <app-catalog-selector [selected]="selectedType()" (catalogChanged)="selectType($event)" />
    <app-catalog-form
      [catalogType]="selectedType()"
      [item]="editing()"
      (submitted)="save($event)"
    />
    <app-catalog-list
      [items]="items()"
      (searchChange)="setSearch($event)"
      (edit)="startEdit($event)"
      (remove)="requestDelete($event)"
    />
    <div>
      <button type="button" [disabled]="skip() === 0" (click)="previousPage()">Anterior</button>
      <span>Mostrando {{ items().length }} de {{ total() }}</span>
      <button
        type="button"
        [disabled]="skip() + items().length >= total()"
        (click)="nextPage()"
      >
        Siguiente
      </button>
    </div>
    @if (error()) {
      <p role="alert">{{ error() }}</p>
    }
    @if (success()) {
      <p role="status">{{ success() }}</p>
    }
    @if (deleting()) {
      <dialog open>
        <p>¿Está seguro?</p>
        <button (click)="cancelDelete()">Cancelar</button>
        <button (click)="delete()">Eliminar</button>
      </dialog>
    }
  `,
})
export class CatalogManagementComponent {
  private readonly service = inject(CatalogService);

  readonly selectedType = signal<CatalogType>(CatalogType.Company);
  readonly items = signal<CatalogItemDto[]>([]);
  readonly editing = signal<CatalogItemDto | null>(null);
  readonly searchCode = signal('');
  readonly searchDescription = signal('');
  readonly error = signal<string | null>(null);
  readonly success = signal<string | null>(null);
  readonly deleting = signal<CatalogItemDto | null>(null);
  readonly skip = signal(0);
  readonly take = signal(10);
  readonly total = signal(0);
  private readonly reload = signal(0);

  constructor() {
    combineLatest([
      toObservable(this.selectedType),
      toObservable(this.searchCode),
      toObservable(this.searchDescription),
      toObservable(this.skip),
      toObservable(this.take),
      toObservable(this.reload),
    ])
      .pipe(
        takeUntilDestroyed(),
        debounceTime(300),
        switchMap(([type, code, description, skip, take]) =>
          this.load(type, code, description, skip, take),
        ),
      )
      .subscribe({
        next: (items) => this.items.set(items),
        error: (err: Error) => this.error.set(err.message),
      });
  }

  selectType(type: CatalogType): void {
    this.selectedType.set(type);
    this.editing.set(null);
    this.searchCode.set('');
    this.searchDescription.set('');
    this.skip.set(0);
    this.success.set(null);
  }

  setSearch(change: { code: string; description: string }): void {
    this.searchCode.set(change.code);
    this.searchDescription.set(change.description);
    this.skip.set(0);
  }

  save(value: { id?: string; code: string; description: string }): void {
    this.error.set(null);
    this.success.set(null);
    const type = this.selectedType();
    const request: Observable<unknown> = value.id
      ? this.service.update(type, value.id, value)
      : this.service.create(type, value);
    request.subscribe({
      next: () => {
        this.editing.set(null);
        this.success.set('Registro guardado correctamente');
        this.reload.update((count) => count + 1);
      },
      error: (err: Error) => this.error.set(err.message),
    });
  }

  startEdit(item: CatalogItemDto): void {
    this.editing.set(item);
  }

  requestDelete(item: CatalogItemDto): void {
    this.deleting.set(item);
  }

  cancelDelete(): void {
    this.deleting.set(null);
  }

  delete(): void {
    const item = this.deleting();
    if (!item) {
      return;
    }

    this.error.set(null);
    this.service.delete(this.selectedType(), item.id).subscribe({
      next: () => {
        this.deleting.set(null);
        this.success.set('Registro eliminado correctamente');
        this.reload.update((count) => count + 1);
      },
      error: (err: Error) => {
        this.deleting.set(null);
        this.error.set(err.message);
      },
    });
  }

  previousPage(): void {
    this.skip.update((skip) => Math.max(0, skip - this.take()));
  }

  nextPage(): void {
    this.skip.update((skip) => skip + this.take());
  }

  private load(
    type: CatalogType,
    code: string,
    description: string,
    skip: number,
    take: number,
  ): Observable<CatalogItemDto[]> {
    this.error.set(null);
    const request$ = code.trim() || description.trim()
      ? this.service.search(type, code.trim(), description.trim(), skip, take)
      : this.service.getList(type, skip, take);

    return request$.pipe(
      map((result) => {
        this.total.set(result.total);
        return result.items;
      }),
    );
  }
}
