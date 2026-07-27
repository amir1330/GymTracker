import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { Router } from '@angular/router';
import { User } from '../models/auth.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = '/api/auth';
  private currentUserSubject = new BehaviorSubject<User | null>(null);

  constructor(private http: HttpClient, private router: Router) {
    const token = localStorage.getItem('token');
    if (token) {
      this.loadUserFromToken(token);
    }
  }

  private loadUserFromToken(token: string): void {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      this.currentUserSubject.next({
        userId: payload.nameid,
        username: payload.name,
        email: payload.email
      });
    } catch {
      localStorage.removeItem('token');
    }
  }

  register(data: { username: string; email: string; password: string; confirmPassword: string; weight?: number; height?: number }): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, data).pipe(
      tap((response: any) => {
        localStorage.setItem('token', response.token);
        this.loadUserFromToken(response.token);
      })
    );
  }

  login(data: { email: string; password: string }): Observable<any> {
    return this.http.post(`${this.apiUrl}/login`, data).pipe(
      tap((response: any) => {
        localStorage.setItem('token', response.token);
        this.loadUserFromToken(response.token);
      })
    );
  }

  logout(): void {
    localStorage.removeItem('token');
    this.currentUserSubject.next(null);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    const token = localStorage.getItem('token');
    if (token && this.isTokenExpired(token)) {
      localStorage.removeItem('token');
      return null;
    }
    return token;
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  private isTokenExpired(token: string): boolean {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      if (!payload.exp) return false;
      return payload.exp * 1000 < Date.now();
    } catch {
      return true;
    }
  }
}
