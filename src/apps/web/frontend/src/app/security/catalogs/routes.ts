import { Routes } from '@angular/router';
import { globalAdminGuard } from './shared/global-admin.guard';

export const catalogRoutes: Routes = [
  {
    path: 'security/catalogs',
    canActivate: [globalAdminGuard],
    loadComponent: () =>
      import('./catalog-management.component').then((m) => m.CatalogManagementComponent),
  },
];
