import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

export const globalAdminGuard: CanActivateFn = () => {
  const router = inject(Router);
  const roles = readRolesFromToken();
  const allowed = roles.some((role) => role.toLowerCase() === 'globaladmin');
  if (!allowed) {
    return router.createUrlTree(['/acceso-denegado']);
  }
  return true;
};

function readRolesFromToken(): string[] {
  if (typeof window === 'undefined') {
    return [];
  }

  const token = window.localStorage.getItem('access_token');
  if (!token) {
    return [];
  }

  try {
    const payload = JSON.parse(atob(token.split('.')[1] ?? '')) as { roles?: string[] };
    return payload.roles ?? [];
  } catch {
    return [];
  }
}
