import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { CatalogService } from './catalog.service';
import { CatalogType } from '../shared/catalog-type';

describe('CatalogService', () => {
  let service: CatalogService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(CatalogService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('creates a company via POST', () => {
    service.create(CatalogType.Company, { code: 'EMP001', description: 'Mi Minería' }).subscribe();

    const req = http.expectOne('/api/catalogs/companies');
    expect(req.request.method).toBe('POST');
    req.flush('some-guid');
  });

  it('lists companies via GET', () => {
    service.getList(CatalogType.Company).subscribe();

    const req = http.expectOne('/api/catalogs/companies?skip=0&take=10');
    expect(req.request.method).toBe('GET');
    req.flush({ items: [], total: 0 });
  });

  it('searches formats by code and description via GET', () => {
    service.search(CatalogType.Format, 'EMP', 'inspección').subscribe();

    const req = http.expectOne(
      '/api/catalogs/formats/search?code=EMP&description=inspecci%C3%B3n&skip=0&take=10',
    );
    expect(req.request.method).toBe('GET');
    req.flush({ items: [], total: 0 });
  });

  it('updates a company via PUT', () => {
    service.update(CatalogType.Company, 'id-1', { code: 'EMP002', description: 'Otra' }).subscribe();

    const req = http.expectOne('/api/catalogs/companies/id-1');
    expect(req.request.method).toBe('PUT');
    req.flush(null);
  });

  it('deletes a discipline via DELETE', () => {
    service.delete(CatalogType.Discipline, 'id-2').subscribe();

    const req = http.expectOne('/api/catalogs/disciplines/id-2');
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });

  it('maps a backend duplicate-code error to a clear message', () => {
    let received = '';
    service.create(CatalogType.Company, { code: 'EMP001', description: 'Mi Minería' }).subscribe({
      error: (error: Error) => {
        received = error.message;
      },
    });

    const req = http.expectOne('/api/catalogs/companies');
    req.flush(
      { error: { code: 'DUPLICATE_CODE', message: 'Código ya existe' } },
      { status: 409, statusText: 'Conflict' },
    );
    expect(received).toBe('Código ya existe');
  });
});
