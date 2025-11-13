import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';

@Injectable({ providedIn: 'root' })
export class ChatbotService {
  private apiUrl = '/api/AIChatbot';

  constructor(private http: HttpClient) {}

  getBotReply(message: string): Observable<string> {
    const token = localStorage.getItem('eshop_token');
    const headers = token ? new HttpHeaders().set('Authorization', `Bearer ${token}`) : new HttpHeaders();
    
    return this.http.post<{reply: string}>(`${this.apiUrl}/chat`, { message }, { headers })
      .pipe(
        map(res => res.reply),
        catchError(() => of('Sorry, I\'m having trouble connecting. Please try again.'))
      );
  }

  getStreamingReply(message: string): Observable<string> {
    const token = localStorage.getItem('eshop_token');
    const headers = token ? new HttpHeaders().set('Authorization', `Bearer ${token}`) : new HttpHeaders();
    
    return this.http.post<{reply: string}>(`${this.apiUrl}/stream`, { message }, { headers })
      .pipe(
        map(res => res.reply),
        catchError(() => of('Sorry, I\'m having trouble connecting. Please try again.'))
      );
  }



  clearHistory(): Observable<any> {
    return this.http.delete(`${this.apiUrl}/clear-history`)
      .pipe(
        catchError(() => of({ message: 'History cleared locally.' }))
      );
  }

  debugAuth(): Observable<any> {
    const token = localStorage.getItem('eshop_token');
    const headers = token ? new HttpHeaders().set('Authorization', `Bearer ${token}`) : new HttpHeaders();
    
    return this.http.get(`${this.apiUrl}/debug-auth`, { headers })
      .pipe(
        catchError((error) => {
          console.error('Debug auth error:', error);
          return of({ error: 'Debug failed', details: error.message });
        })
      );
  }
}
