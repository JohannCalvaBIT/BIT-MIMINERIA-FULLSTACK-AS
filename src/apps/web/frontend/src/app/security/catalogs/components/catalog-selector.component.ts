import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { CatalogType } from '../shared/catalog.types';

@Component({
  selector: 'app-catalog-selector',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <label>
      Catálogo
      <select [value]="selected()" (change)="changed($event)">
        <option value="company">Empresa</option>
        <option value="format">Formato</option>
        <option value="discipline">Disciplina</option>
      </select>
    </label>
  `,
})
export class CatalogSelectorComponent {
  readonly selected = input.required<CatalogType>();
  readonly catalogChanged = output<CatalogType>();

  changed(event: Event): void {
    const value = (event.target as HTMLSelectElement).value as CatalogType;
    this.catalogChanged.emit(value);
  }
}
