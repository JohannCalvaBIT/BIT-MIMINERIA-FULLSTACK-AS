import { ChangeDetectionStrategy, Component, effect, inject, signal } from '@angular/core';
import { toObservable } from '@angular/core/rxjs-interop';
import { debounceTime, switchMap } from 'rxjs';
import { CatalogListComponent } from './components/catalog-list.component';
import { CatalogSelectorComponent } from './components/catalog-selector.component';
import { CatalogFormComponent } from './components/catalog-form.component';
import { CatalogService } from './services/catalog.service';
import { CatalogItemDto, CatalogType } from './shared/catalog.types';

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
    <input #search type="search" placeholder="Buscar por código o descripción" (input)="setSearch(search.value)" />
    <app-catalog-list
      [items]="items()"
      (edit)="startEdit($event)"
      (remove)="requestDelete($event)"
    />
    @if (error()) {
      <p role="alert">{{ error() }}</p>
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
  readonly searchTerm = signal('');
  readonly error = signal<string | null>(null);
  readonly deleting = signal<CatalogItemDto | null>(null);

  constructor() {
    effect(() => {
      const type = this.selectedType();
      const term = this.searchTerm();
      void this.load(type, term);
    });

    toObservable(this.searchTerm)
      .pipe(debounceTime(300))
      .subscribe(() => this.runSearch());
  }

  selectType(type: CatalogType): void {
    this.selectedType.set(type);
    this.editing.set(null);
  }

  setSearch(term: string): void {
    this.searchTerm.set(term);
  }

  save(value: { id?: string; code: string; description: string }): void {
    this.error.set(null);
    const type = this.selectedType();
    const request = value.id
      ? this.service.update(type, value.id, value)
      : this.service.create(type, value);
    request.subscribe({
      next: () => {
        this.editing.set(null);
        void this.load(type, this.searchTerm());
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
        void this.load(this.selectedType(), this.searchTerm());
      },
      error: (err: Error) => {
        this.deleting.set(null);
        this.error.set(err.message);
      },
    });
  }

  private runSearch(): void {
    void this.load(this.selectedType(), this.searchTerm());
  }

  private async load(type: CatalogType, term: string): Promise<void> {
    this.error.set(null);
    try {
      const result = term
        ? await new Promise<Awaited<ReturnType<CatalogService['search']>>>((resolve, reject) =>
            this.service.search(type, term, '').subscribe({ next: resolve, error: reject }),
          )
        : await new Promise<Awaited<ReturnType<CatalogService['getList']>>>((resolve, reject) =>
            this.service.getList(type).subscribe({ next: resolve, error: reject }),
          );
      this.items.set(result.items);
    } catch (err) {
      this.error.set((err as Error).message);
    }
  }
}
