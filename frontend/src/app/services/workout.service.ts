import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Workout } from '../models/workout.model';

@Injectable({
  providedIn: 'root'
})
export class WorkoutService {
  private apiUrl = '/api/workouts';

  constructor(private http: HttpClient) {}

  getAll(): Observable<Workout[]> {
    return this.http.get<Workout[]>(this.apiUrl);
  }

  getById(id: number): Observable<Workout> {
    return this.http.get<Workout>(`${this.apiUrl}/${id}`);
  }

  create(workout: Partial<Workout>): Observable<Workout> {
    const body = {
      date: workout.date,
      notes: workout.notes,
      bodyWeight: workout.bodyWeight,
      exercises: (workout.workoutExercises || []).map(we => ({
        exerciseId: we.exerciseId,
        sets: we.sets,
        reps: we.reps,
        weight: we.weight,
        duration: we.duration,
        durationUnit: we.durationUnit,
        restTime: we.restTime
      }))
    };
    return this.http.post<Workout>(this.apiUrl, body);
  }

  createFromPreset(presetId: number): Observable<Workout> {
    return this.http.post<Workout>(`${this.apiUrl}/from-preset/${presetId}`, {});
  }

  update(id: number, workout: Partial<Workout>): Observable<Workout> {
    const body = {
      date: workout.date,
      notes: workout.notes,
      bodyWeight: workout.bodyWeight,
      exercises: (workout.workoutExercises || []).map(we => ({
        exerciseId: we.exerciseId,
        sets: we.sets,
        reps: we.reps,
        weight: we.weight,
        duration: we.duration,
        durationUnit: we.durationUnit,
        restTime: we.restTime
      }))
    };
    return this.http.put<Workout>(`${this.apiUrl}/${id}`, body);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
