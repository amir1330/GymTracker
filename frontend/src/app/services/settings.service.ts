import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface UserSettings {
  theme?: string;
  language?: string;
}

@Injectable({
  providedIn: 'root'
})
export class SettingsService {
  private apiUrl = '/api/user';

  constructor(private http: HttpClient) {}

  getSettings(): Observable<UserSettings> {
    return this.http.get<UserSettings>(`${this.apiUrl}/settings`);
  }

  updateSettings(data: { theme?: string; language?: string }): Observable<any> {
    return this.http.put(`${this.apiUrl}/settings`, data);
  }
}
