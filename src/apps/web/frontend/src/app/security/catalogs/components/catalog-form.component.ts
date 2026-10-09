import { ChangeDetectionStrategy, Component, computed, effect, inject, input, output } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CatalogType } from '../shared/catalog-type';

@Component({
  selector: 'app-catalog-form',
  standalone: true,
  imports: [ReactiveFormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <form [formGroup]="form" (ngSubmit)="submit()">
      <label>
        Código
        <input formControlName="code" type="text" [disabled]="item()?.isInUse === true" />
        @if (form.controls.code.touched && form.controls.code.hasError('required')) {
          <small role="alert">El código es obligatorio</small>
        }
        @if (form.controls.code.touched && form.controls.code.hasError('maxlength')) {
          <small role="alert">El código no puede superar 150 caracteres</small>
        }
        @if (item()?.isInUse === true) {
          <small>El código no se puede editar porque el registro está en uso</small>
        }
      </label>
      <label>
        Descripción
        <input formControlName="description" type="text" />
        @if (form.controls.description.touched && form.controls.description.hasError('required')) {
          <small role="alert">La descripción es obligatoria</small>
        }
        @if (form.controls.description.touched && form.controls.description.hasError('maxlength')) {
          <small role="alert">La descripción no puede superar 250 caracteres</small>
        }
      </label>
      <button type="submit" [disabled]="form.invalid">Guardar</button>
    </form>
  `,
})
export class CatalogFormComponent {
  private readonly fb = inject(FormBuilder);

  readonly catalogType = input.required<CatalogType>();
  readonly item = input<{ id?: string; code: string; description: string; isInUse: boolean } | null>(null);
  readonly submitted = output<{ id?: string; code: string; description: string }>();

  readonly form = this.fb.group({
    code: ['', [Validators.required, Validators.maxLength(150)]],
    description: ['', [Validators.required, Validators.maxLength(250)]],
  });

  readonly title = computed(() => (this.item()?.id ? 'Editar registro' : 'Crear registro'));

  constructor() {
    effect(() => {
      const item = this.item();
      if (item) {
        this.form.setValue({ code: item.code, description: item.description });
        if (item.isInUse) {
          this.form.controls.code.disable();
        } else {
          this.form.controls.code.enable();
        }
      } else {
        this.form.reset();
        this.form.controls.code.enable();
      }
    });
  }

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
