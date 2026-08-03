import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { RouterModule } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { WorkoutService } from '../../services/workout.service';
import { Workout, WorkoutExercise } from '../../models/workout.model';
import { TranslationService } from '../../services/translation.service';

@Component({
  selector: 'app-workout-list',
  standalone: true,
  imports: [RouterModule, TranslatePipe],
  templateUrl: './workout-list.html'
})
export class WorkoutList implements OnInit {
  workouts: Workout[] = [];
  loading = true;

  constructor(
    private workoutService: WorkoutService,
    private cdr: ChangeDetectorRef,
    private translationService: TranslationService
  ) {}

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
    const message = this.translationService.instant('common.confirmDelete', { item: this.translationService.instant('workout.title') });
    if (confirm(message)) {
      this.workoutService.delete(id).subscribe(() => this.loadWorkouts());
    }
  }

  formatDate(dateStr: string): string {
    const localeMap: Record<string, string> = { kz: 'kk-KZ', ru: 'ru-RU', en: 'en-US' };
    const locale = localeMap[this.translationService.getCurrentLanguage()] || 'en-US';
    return new Date(dateStr).toLocaleDateString(locale, {
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
