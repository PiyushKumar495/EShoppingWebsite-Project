import { Injectable } from '@angular/core';
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    console.log('Interceptor is running for:', req.url);
  const token = localStorage.getItem('eshop_token');
  console.log('Intercepting request:', req.url);
  console.log('Token:', token);

  if (token) {
    const cloned = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
    console.log('Modified request:', cloned.headers.get('Authorization'));
    return next.handle(cloned);
  }

  return next.handle(req);
}

}
