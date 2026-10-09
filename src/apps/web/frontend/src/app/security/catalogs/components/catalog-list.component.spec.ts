import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { CatalogListComponent } from './catalog-list.component';
import { CatalogItemDto } from '../shared/catalog.types';

describe('CatalogListComponent', () => {
  let fixture: ComponentFixture<CatalogListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CatalogListComponent],
      providers: [provideZonelessChangeDetection()],
    }).compileComponents();

    fixture = TestBed.createComponent(CatalogListComponent);
  });

  it('shows an empty state when there are no items', async () => {
    fixture.componentRef.setInput('items', [] as CatalogItemDto[]);
    await fixture.whenStable();
    fixture.detectChanges();

    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(text).toContain('No hay registros');
  });

  it('lists items and disables edit/delete when the item is in use', async () => {
    fixture.componentRef.setInput('items', [
      {
        id: 'in-use',
        code: 'EMP001',
        description: 'Mi Minería',
        isInUse: true,
        createdAt: '2026-10-09T00:00:00Z',
        modifiedAt: '2026-10-09T00:00:00Z',
      },
    ] as CatalogItemDto[]);
    await fixture.whenStable();
    fixture.detectChanges();

    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(text).toContain('EMP001');
    expect(text).toContain('En uso');

    const buttons = Array.from(
      (fixture.nativeElement as HTMLElement).querySelectorAll('button'),
    ) as HTMLButtonElement[];
    expect(buttons.every((button) => button.disabled)).toBe(true);
  });

  it('emits search criteria when either search input changes', async () => {
    const emitted: { code: string; description: string }[] = [];
    fixture.componentInstance.searchChange.subscribe((value) => emitted.push(value));
    fixture.componentRef.setInput('items', [] as CatalogItemDto[]);
    await fixture.whenStable();
    fixture.detectChanges();

    const [codeInput, descriptionInput] = Array.from(
      (fixture.nativeElement as HTMLElement).querySelectorAll('input'),
    ) as HTMLInputElement[];

    codeInput.value = 'EMP';
    codeInput.dispatchEvent(new Event('input'));
    expect(emitted).toEqual([{ code: 'EMP', description: '' }]);

    descriptionInput.value = 'inspección';
    descriptionInput.dispatchEvent(new Event('input'));
    expect(emitted.at(-1)).toEqual({ code: 'EMP', description: 'inspección' });
  });
});
