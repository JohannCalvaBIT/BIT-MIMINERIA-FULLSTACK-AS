export interface CatalogItemDto {
  id: string;
  code: string;
  description: string;
  isInUse: boolean;
  createdAt: string;
  modifiedAt: string;
}

export interface PagedResult<T> {
  items: T[];
  total: number;
}

export interface CatalogWriteDto {
  code: string;
  description: string;
}
