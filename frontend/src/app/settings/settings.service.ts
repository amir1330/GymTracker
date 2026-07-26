import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface UserProfile {
  id: number;
  userName: string;
  email: string;
  weight?: number;
  height?: number;
  settings?: {
    theme: string;
  };
}

@Injectable({
  providedIn: 'root'
})
export class SettingsService {
  private apiUrl = '/api/user';

  constructor(private http: HttpClient) {}

  getProfile(): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${this.apiUrl}/profile`);
  }

  updateProfile(data: { weight?: number; height?: number }): Observable<any> {
    return this.http.put(`${this.apiUrl}/profile`, data);
  }

  updateSettings(data: { theme?: string }): Observable<any> {
    return this.http.put(`${this.apiUrl}/settings`, data);
  }
}
