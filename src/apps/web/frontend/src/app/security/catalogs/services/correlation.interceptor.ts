import { HttpInterceptorFn } from '@angular/common/http';
import { v4 as uuidv4 } from './uuid';

export const correlationInterceptor: HttpInterceptorFn = (req, next) => {
  const correlationId = sessionStorage.getItem('correlation_id') ?? uuidv4();
  sessionStorage.setItem('correlation_id', correlationId);
  return next(
    req.clone({
      setHeaders: { 'X-Correlation-Id': correlationId },
    }),
  );
};
