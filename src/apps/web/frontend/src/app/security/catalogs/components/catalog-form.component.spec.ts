import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { CatalogFormComponent } from './catalog-form.component';
import { CatalogType } from '../shared/catalog.types';

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
    expect(button.disabled).toBeTrue();
  });
});
