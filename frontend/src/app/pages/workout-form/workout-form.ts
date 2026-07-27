import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { WorkoutService } from '../../services/workout.service';
import { Workout, WorkoutExercise } from '../../models/workout.model';
import { ExerciseService } from '../../services/exercise.service';
import { Exercise } from '../../models/exercise.model';
import { PresetService } from '../../services/preset.service';
import { Preset } from '../../models/preset.model';

@Component({
  selector: 'app-workout-form',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './workout-form.html'
})
export class WorkoutForm implements OnInit {
  workout: Partial<Workout> = {
    date: new Date().toISOString().split('T')[0],
    notes: '',
    bodyWeight: undefined,
    workoutExercises: []
  };
  exercises: Exercise[] = [];
  presets: Preset[] = [];
  isEdit = false;
  error = '';

  constructor(
    private workoutService: WorkoutService,
    private exerciseService: ExerciseService,
    private presetService: PresetService,
    private router: Router,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.exerciseService.getAll().subscribe(exercises => { this.exercises = exercises; this.cdr.markForCheck(); });
    this.presetService.getAll().subscribe(presets => { this.presets = presets; this.cdr.markForCheck(); });

    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEdit = true;
      this.workoutService.getById(+id).subscribe({
        next: (workout) => { this.workout = workout; this.cdr.markForCheck(); },
        error: () => this.router.navigate(['/workouts'])
      });
    }
  }

  loadPreset(preset: Preset): void {
    this.workout.workoutExercises = preset.presetExercises.map(pe => {
      const we: WorkoutExercise = {
        exerciseId: pe.exerciseId,
        sets: pe.defaultSets,
        reps: pe.defaultReps,
        weight: pe.defaultWeight,
        duration: pe.defaultDuration
      };
      return we;
    });
    this.cdr.markForCheck();
  }

  addExercise(): void {
    const exercise = this.exercises[0];
    const we: WorkoutExercise = {
      exerciseId: exercise?.id || 0,
      sets: 3,
      reps: 10,
      weight: undefined,
      duration: undefined
    };
    if (exercise?.isDuration) {
      we.sets = 0;
      we.reps = 0;
      we.duration = 30;
    }
    this.workout.workoutExercises?.push(we);
  }

  getExercise(exerciseId?: number): Exercise | undefined {
    if (!exerciseId) return undefined;
    return this.exercises.find(e => e.id === exerciseId);
  }

  onExerciseTypeChange(we: Partial<WorkoutExercise>): void {
    const exercise = this.getExercise(we.exerciseId);
    if (exercise?.isDuration) {
      we.sets = 0;
      we.reps = 0;
      we.weight = undefined;
      if (!we.duration) we.duration = 30;
    } else {
      we.duration = undefined;
      if (!we.sets) we.sets = 3;
      if (!we.reps) we.reps = 10;
    }
  }

  removeExercise(index: number): void {
    this.workout.workoutExercises?.splice(index, 1);
  }

  private extractError(err: any): string {
    const body = err.error;
    if (typeof body === 'string') return body;
    if (body?.message) return body.message;
    if (Array.isArray(body)) return body.map((e: any) => e.description).join('. ');
    return 'Operation failed';
  }

  onSubmit(): void {
    this.error = '';
    if (this.isEdit && this.workout.id) {
      this.workoutService.update(this.workout.id, this.workout).subscribe({
        next: () => this.router.navigate(['/workouts']),
        error: (err) => { this.error = this.extractError(err); this.cdr.markForCheck(); }
      });
    } else {
      this.workoutService.create(this.workout).subscribe({
        next: () => this.router.navigate(['/workouts']),
        error: (err) => { this.error = this.extractError(err); this.cdr.markForCheck(); }
      });
    }
  }
}
