import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { catchError, Observable, throwError } from 'rxjs';
import { CatalogItemDto, CatalogWriteDto, PagedResult } from '../shared/catalog.types';
import { CatalogType } from '../shared/catalog-type';

@Injectable({ providedIn: 'root' })
export class CatalogService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/catalogs';

  getList(type: CatalogType, skip = 0, take = 10): Observable<PagedResult<CatalogItemDto>> {
    return this.http
      .get<PagedResult<CatalogItemDto>>(`${this.baseUrl}/${this.resource(type)}`, {
        params: { skip, take },
      })
      .pipe(catchError((error: HttpErrorResponse) => throwError(() => this.messageFor(error))));
  }

  search(
    type: CatalogType,
    code: string,
    description: string,
    skip = 0,
    take = 10,
  ): Observable<PagedResult<CatalogItemDto>> {
    return this.http
      .get<PagedResult<CatalogItemDto>>(`${this.baseUrl}/${this.resource(type)}/search`, {
        params: { code, description, skip, take },
      })
      .pipe(catchError((error: HttpErrorResponse) => throwError(() => this.messageFor(error))));
  }

  create(type: CatalogType, dto: CatalogWriteDto): Observable<string> {
    return this.http
      .post<string>(`${this.baseUrl}/${this.resource(type)}`, dto)
      .pipe(catchError((error: HttpErrorResponse) => throwError(() => this.messageFor(error))));
  }

  update(type: CatalogType, id: string, dto: CatalogWriteDto): Observable<void> {
    return this.http
      .put<void>(`${this.baseUrl}/${this.resource(type)}/${id}`, dto)
      .pipe(catchError((error: HttpErrorResponse) => throwError(() => this.messageFor(error))));
  }

  delete(type: CatalogType, id: string): Observable<void> {
    return this.http
      .delete<void>(`${this.baseUrl}/${this.resource(type)}/${id}`)
      .pipe(catchError((error: HttpErrorResponse) => throwError(() => this.messageFor(error))));
  }

  private messageFor(error: HttpErrorResponse): Error {
    const body = error.error as { error?: { message?: string }; message?: string } | undefined;
    return new Error(
      body?.error?.message ?? body?.message ?? 'Ocurrió un error al procesar la operación',
    );
  }

  private resource(type: CatalogType): string {
    switch (type) {
      case CatalogType.Company:
        return 'companies';
      case CatalogType.Format:
        return 'formats';
      case CatalogType.Discipline:
        return 'disciplines';
    }

    throw new Error(`Tipo de catálogo no soportado: ${type}`);
  }
}
