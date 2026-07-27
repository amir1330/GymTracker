import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { PresetService } from '../../services/preset.service';
import { Preset, PresetExercise } from '../../models/preset.model';
import { ExerciseService } from '../../services/exercise.service';
import { Exercise } from '../../models/exercise.model';

@Component({
  selector: 'app-preset-form',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './preset-form.html'
})
export class PresetForm implements OnInit {
  preset: Partial<Preset> = {
    name: '',
    presetExercises: []
  };
  exercises: Exercise[] = [];
  isEdit = false;
  error = '';

  constructor(
    private presetService: PresetService,
    private exerciseService: ExerciseService,
    private router: Router,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.exerciseService.getAll().subscribe(exercises => { this.exercises = exercises; this.cdr.markForCheck(); });

    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEdit = true;
      this.presetService.getById(+id).subscribe({
        next: (preset) => { this.preset = preset; this.cdr.markForCheck(); },
        error: () => this.router.navigate(['/presets'])
      });
    }
  }

  addExercise(): void {
    const exercise = this.exercises[0];
    const pe: PresetExercise = {
      exerciseId: exercise?.id || 0,
      defaultSets: 3,
      defaultReps: 10,
      defaultWeight: undefined,
      defaultDuration: undefined
    };
    if (exercise?.isDuration) {
      pe.defaultSets = 0;
      pe.defaultReps = 0;
      pe.defaultDuration = 30;
    }
    this.preset.presetExercises?.push(pe);
  }

  removeExercise(index: number): void {
    this.preset.presetExercises?.splice(index, 1);
  }

  getExercise(exerciseId?: number): Exercise | undefined {
    if (!exerciseId) return undefined;
    const id = Number(exerciseId);
    return this.exercises.find(e => e.id === id);
  }

  onExerciseTypeChange(pe: PresetExercise): void {
    const exercise = this.getExercise(pe.exerciseId);
    if (exercise?.isDuration) {
      pe.defaultSets = 0;
      pe.defaultReps = 0;
      pe.defaultWeight = undefined;
      if (!pe.defaultDuration) pe.defaultDuration = 30;
    } else {
      pe.defaultDuration = undefined;
      if (!pe.defaultSets) pe.defaultSets = 3;
      if (!pe.defaultReps) pe.defaultReps = 10;
    }
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
    if (this.isEdit && this.preset.id) {
      this.presetService.update(this.preset.id, this.preset).subscribe({
        next: () => this.router.navigate(['/presets']),
        error: (err) => { this.error = this.extractError(err); this.cdr.markForCheck(); }
      });
    } else {
      this.presetService.create(this.preset).subscribe({
        next: () => this.router.navigate(['/presets']),
        error: (err) => { this.error = this.extractError(err); this.cdr.markForCheck(); }
      });
    }
  }
}
