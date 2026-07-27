import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SettingsService {
  private apiUrl = '/api/user';

  constructor(private http: HttpClient) {}

  updateSettings(data: { theme?: string }): Observable<any> {
    return this.http.put(`${this.apiUrl}/settings`, data);
  }
}
