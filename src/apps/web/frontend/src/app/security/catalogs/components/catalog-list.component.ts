import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { CatalogItemDto } from '../shared/catalog.types';

@Component({
  selector: 'app-catalog-list',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <input #search type="search" placeholder="Buscar por código o descripción" />
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
  readonly edit = output<CatalogItemDto>();
  readonly remove = output<CatalogItemDto>();
}
