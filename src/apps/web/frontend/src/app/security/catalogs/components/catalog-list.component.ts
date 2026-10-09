import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { CatalogItemDto } from '../shared/catalog.types';

@Component({
  selector: 'app-catalog-list',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div>
      <input
        #code
        type="search"
        placeholder="Buscar por código"
        (input)="searchChanged(code.value, description.value)"
      />
      <input
        #description
        type="search"
        placeholder="Buscar por descripción"
        (input)="searchChanged(code.value, description.value)"
      />
    </div>
    @if (items().length === 0) {
      <p>No hay registros que coincidan con la búsqueda</p>
    } @else {
      @for (item of items(); track item.id) {
        <div>
          <span>{{ item.code }} — {{ item.description }}</span>
          @if (item.isInUse) {
            <span>En uso</span>
          }
          <button [disabled]="item.isInUse" (click)="edit.emit(item)">Editar código</button>
          <button [disabled]="item.isInUse" (click)="remove.emit(item)">Eliminar</button>
        </div>
      }
    }
  `,
})
export class CatalogListComponent {
  readonly items = input.required<CatalogItemDto[]>();
  readonly searchChange = output<{ code: string; description: string }>();
  readonly edit = output<CatalogItemDto>();
  readonly remove = output<CatalogItemDto>();

  searchChanged(code: string, description: string): void {
    this.searchChange.emit({ code, description });
  }
}
