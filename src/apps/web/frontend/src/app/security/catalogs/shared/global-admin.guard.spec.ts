import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { globalAdminGuard } from './global-admin.guard';

describe('globalAdminGuard', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideRouter([
          { path: 'acceso-denegado', component: class {} },
        ]),
      ],
    });
  });

  it('denies access when no token is present', () => {
    window.localStorage.removeItem('access_token');
    const result = TestBed.runInInjectionContext(() => globalAdminGuard({} as never, [] as never));
    expect(result).not.toBeTrue();
  });
});
