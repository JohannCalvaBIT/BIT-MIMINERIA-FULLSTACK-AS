import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { CatalogFormComponent } from './catalog-form.component';
import { CatalogType } from '../shared/catalog-type';

describe('CatalogFormComponent', () => {
  let fixture: ComponentFixture<CatalogFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CatalogFormComponent, ReactiveFormsModule],
      providers: [provideZonelessChangeDetection()],
    }).compileComponents();

    fixture = TestBed.createComponent(CatalogFormComponent);
    fixture.componentRef.setInput('catalogType', CatalogType.Company);
    fixture.componentRef.setInput('item', null);
    await fixture.whenStable();
  });

  it('creates the component', () => {
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('disables submit when the form is invalid', () => {
    fixture.detectChanges();
    const button = (fixture.nativeElement as HTMLElement).querySelector('button') as HTMLButtonElement;
    expect(button.disabled).toBe(true);
  });

  it('shows validation messages for code and description after they are touched', () => {
    fixture.componentInstance.form.controls.code.markAsTouched();
    fixture.componentInstance.form.controls.description.markAsTouched();
    fixture.detectChanges();

    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(text).toContain('El código es obligatorio');
    expect(text).toContain('La descripción es obligatoria');
  });

  it('shows maxLength validation messages when limits are exceeded', () => {
    fixture.componentInstance.form.setValue({
      code: 'a'.repeat(151),
      description: 'b'.repeat(251),
    });
    fixture.componentInstance.form.markAllAsTouched();
    fixture.detectChanges();

    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(text).toContain('El código no puede superar 150 caracteres');
    expect(text).toContain('La descripción no puede superar 250 caracteres');
  });

  it('emits the form value when the form is valid', () => {
    const emitted: { id?: string; code: string; description: string }[] = [];
    fixture.componentInstance.submitted.subscribe((value) => emitted.push(value));
    fixture.componentInstance.form.setValue({ code: 'EMP001', description: 'Mi Minería' });

    fixture.componentInstance.submit();

    expect(emitted).toEqual([{ id: undefined, code: 'EMP001', description: 'Mi Minería' }]);
  });

  it('disables the code field when the item is in use', async () => {
    fixture.componentRef.setInput('item', {
      id: '123e4567-e89b-12d3-a456-426614174000',
      code: 'EMP001',
      description: 'Mi Minería',
      isInUse: true,
    });
    await fixture.whenStable();
    fixture.detectChanges();

    const codeInput = (fixture.nativeElement as HTMLElement).querySelector(
      'input[formControlName="code"]',
    ) as HTMLInputElement;
    expect(codeInput.disabled).toBe(true);
  });
});
