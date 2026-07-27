import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Preset } from '../models/preset.model';

@Injectable({
  providedIn: 'root'
})
export class PresetService {
  private apiUrl = '/api/presets';

  constructor(private http: HttpClient) {}

  getAll(): Observable<Preset[]> {
    return this.http.get<Preset[]>(this.apiUrl);
  }

  getById(id: number): Observable<Preset> {
    return this.http.get<Preset>(`${this.apiUrl}/${id}`);
  }

  create(preset: Partial<Preset>): Observable<Preset> {
    const body = {
      name: preset.name,
      exercises: (preset.presetExercises || []).map(pe => ({
        exerciseId: pe.exerciseId,
        defaultSets: pe.defaultSets,
        defaultReps: pe.defaultReps,
        defaultWeight: pe.defaultWeight,
        defaultDuration: pe.defaultDuration
      }))
    };
    return this.http.post<Preset>(this.apiUrl, body);
  }

  update(id: number, preset: Partial<Preset>): Observable<Preset> {
    const body = {
      name: preset.name,
      exercises: (preset.presetExercises || []).map(pe => ({
        exerciseId: pe.exerciseId,
        defaultSets: pe.defaultSets,
        defaultReps: pe.defaultReps,
        defaultWeight: pe.defaultWeight,
        defaultDuration: pe.defaultDuration
      }))
    };
    return this.http.put<Preset>(`${this.apiUrl}/${id}`, body);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
