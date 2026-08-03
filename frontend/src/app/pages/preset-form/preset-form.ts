import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { PresetService } from '../../services/preset.service';
import { Preset, PresetExercise } from '../../models/preset.model';
import { ExerciseService } from '../../services/exercise.service';
import { Exercise } from '../../models/exercise.model';
import { TranslationService } from '../../services/translation.service';

@Component({
  selector: 'app-preset-form',
  standalone: true,
  imports: [FormsModule, RouterModule, TranslatePipe],
  templateUrl: './preset-form.html'
})
export class PresetForm implements OnInit {
  preset: Partial<Preset> = {
    name: '',
    presetExercises: []
  };
  exercises: Exercise[] = [];
  isEdit = false;

  constructor(
    private presetService: PresetService,
    private exerciseService: ExerciseService,
    private router: Router,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef,
    private translationService: TranslationService
  ) {}

  ngOnInit(): void {
    this.exerciseService.getAll().subscribe(exercises => { this.exercises = exercises; this.cdr.markForCheck(); });

    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEdit = true;
      this.presetService.getById(+id).subscribe(preset => {
        this.preset = preset;
        this.cdr.markForCheck();
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

  durationUnitLabel(unit?: string): string {
    const key = unit === 'minutes' ? 'workout.minutes' : unit === 'hours' ? 'workout.hours' : 'workout.seconds';
    return this.translationService.instant(key);
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

  onSubmit(): void {
    if (this.isEdit && this.preset.id) {
      this.presetService.update(this.preset.id, this.preset).subscribe(() => this.router.navigate(['/presets']));
    } else {
      this.presetService.create(this.preset).subscribe(() => this.router.navigate(['/presets']));
    }
  }
}
