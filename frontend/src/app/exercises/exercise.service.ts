import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Exercise {
  id: number;
  name: string;
  muscleGroup: string;
  isDuration: boolean;
  isDefault: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class ExerciseService {
  private apiUrl = '/api/exercises';

  constructor(private http: HttpClient) {}

  getAll(): Observable<Exercise[]> {
    return this.http.get<Exercise[]>(this.apiUrl);
  }

  getById(id: number): Observable<Exercise> {
    return this.http.get<Exercise>(`${this.apiUrl}/${id}`);
  }

  create(exercise: Partial<Exercise>): Observable<Exercise> {
    return this.http.post<Exercise>(this.apiUrl, exercise);
  }

  update(id: number, exercise: Partial<Exercise>): Observable<Exercise> {
    return this.http.put<Exercise>(`${this.apiUrl}/${id}`, exercise);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
