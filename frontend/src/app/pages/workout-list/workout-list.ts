import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { WorkoutService } from '../../services/workout.service';
import { Workout, WorkoutExercise } from '../../models/workout.model';

@Component({
  selector: 'app-workout-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './workout-list.html'
})
export class WorkoutList implements OnInit {
  workouts: Workout[] = [];
  loading = true;
  error = '';

  constructor(private workoutService: WorkoutService, private router: Router, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.loadWorkouts();
  }

  loadWorkouts(): void {
    this.loading = true;
    this.error = '';
    this.workoutService.getAll().subscribe({
      next: (workouts) => {
        this.workouts = workouts;
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.error = this.extractError(err);
        this.loading = false;
        this.cdr.markForCheck();
      }
    });
  }

  deleteWorkout(id: number): void {
    if (confirm('Are you sure you want to delete this workout?')) {
      this.error = '';
      this.workoutService.delete(id).subscribe({
        next: () => this.loadWorkouts(),
        error: (err) => {
          this.error = this.extractError(err);
          this.cdr.markForCheck();
        }
      });
    }
  }

  formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString('en-US', {
      weekday: 'short',
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }

  formatWorkoutExercise(we: WorkoutExercise): string {
    if (we.duration) {
      const unit = we.durationUnit || 'seconds';
      return `(${we.duration}${unit === 'seconds' ? 's' : unit === 'minutes' ? 'min' : 'hr'})`;
    }
    const parts = [`${we.sets}x${we.reps}`];
    if (we.weight) {
      parts.push(`@${we.weight}kg`);
    }
    return `(${parts.join(' ')})`;
  }

  private extractError(err: any): string {
    const body = err.error;
    if (typeof body === 'string') return body;
    if (body?.message) return body.message;
    if (Array.isArray(body)) return body.map((e: any) => e.description).join('. ');
    return 'Operation failed';
  }
}
