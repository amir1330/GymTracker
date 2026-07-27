import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UserProfile } from '../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class SettingsService {
  private apiUrl = '/api/user';

  constructor(private http: HttpClient) {}

  getProfile(): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${this.apiUrl}/profile`);
  }

  updateSettings(data: { theme?: string }): Observable<any> {
    return this.http.put(`${this.apiUrl}/settings`, data);
  }
}
