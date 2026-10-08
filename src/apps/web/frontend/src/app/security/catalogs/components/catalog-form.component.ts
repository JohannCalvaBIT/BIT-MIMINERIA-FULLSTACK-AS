import { ChangeDetectionStrategy, Component, computed, inject, input, output } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CatalogType } from '../shared/catalog.types';

@Component({
  selector: 'app-catalog-form',
  standalone: true,
  imports: [ReactiveFormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <form [formGroup]="form" (ngSubmit)="submit()">
      <label>
        Código
        <input formControlName="code" type="text" />
      </label>
      <label>
        Descripción
        <input formControlName="description" type="text" />
      </label>
      <button type="submit" [disabled]="form.invalid">Guardar</button>
    </form>
  `,
})
export class CatalogFormComponent {
  private readonly fb = inject(FormBuilder);

  readonly catalogType = input.required<CatalogType>();
  readonly item = input<{ id?: string; code: string; description: string } | null>(null);
  readonly submitted = output<{ id?: string; code: string; description: string }>();

  readonly form = this.fb.group({
    code: ['', [Validators.required, Validators.maxLength(150)]],
    description: ['', [Validators.required, Validators.maxLength(250)]],
  });

  readonly title = computed(() => (this.item()?.id ? 'Editar registro' : 'Crear registro'));

  submit(): void {
    if (this.form.invalid) {
      return;
    }

    const value = this.form.value;
    this.submitted.emit({
      id: this.item()?.id,
      code: value.code ?? '',
      description: value.description ?? '',
    });
  }
}
