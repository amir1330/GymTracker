import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { RouterModule } from '@angular/router';
import { WorkoutService } from '../../services/workout.service';
import { Workout, WorkoutExercise } from '../../models/workout.model';

@Component({
  selector: 'app-workout-list',
  standalone: true,
  imports: [RouterModule],
  templateUrl: './workout-list.html'
})
export class WorkoutList implements OnInit {
  workouts: Workout[] = [];
  loading = true;

  constructor(private workoutService: WorkoutService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.loadWorkouts();
  }

  loadWorkouts(): void {
    this.loading = true;
    this.workoutService.getAll().subscribe(workouts => {
      this.workouts = workouts;
      this.loading = false;
      this.cdr.markForCheck();
    });
  }

  deleteWorkout(id: number): void {
    if (confirm('Are you sure you want to delete this workout?')) {
      this.workoutService.delete(id).subscribe(() => this.loadWorkouts());
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
}
