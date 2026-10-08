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
});
