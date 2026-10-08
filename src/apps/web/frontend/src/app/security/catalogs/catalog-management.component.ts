import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
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
    <app-catalog-list
      [items]="items()"
      (edit)="startEdit($event)"
      (remove)="confirmDelete($event)"
    />
  `,
})
export class CatalogManagementComponent {
  private readonly service = inject(CatalogService);

  readonly selectedType = signal<CatalogType>(CatalogType.Company);
  readonly items = signal<CatalogItemDto[]>([]);
  readonly editing = signal<CatalogItemDto | null>(null);
  readonly searchTerm = signal('');

  readonly filteredItems = computed(() => {
    const term = this.searchTerm().toLowerCase();
    if (!term) {
      return this.items();
    }
    return this.items().filter(
      (item) =>
        item.code.toLowerCase().includes(term) ||
        item.description.toLowerCase().includes(term),
    );
  });

  constructor() {
    this.load();
  }

  selectType(type: CatalogType): void {
    this.selectedType.set(type);
    this.editing.set(null);
    this.load();
  }

  load(): void {
    this.service.getList(this.selectedType()).subscribe((result) => {
      this.items.set(result.items);
    });
  }

  save(value: { id?: string; code: string; description: string }): void {
    if (value.id) {
      this.service.update(this.selectedType(), value.id, value).subscribe(() => this.load());
    } else {
      this.service.create(this.selectedType(), value).subscribe(() => this.load());
    }
  }

  startEdit(item: CatalogItemDto): void {
    this.editing.set(item);
  }

  confirmDelete(item: CatalogItemDto): void {
    const confirmed = window.confirm('¿Está seguro?');
    if (confirmed) {
      this.service.delete(this.selectedType(), item.id).subscribe(() => this.load());
    }
  }
}
