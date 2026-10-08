import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { CatalogService } from './catalog.service';
import { CatalogType } from '../shared/catalog.types';

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
});
