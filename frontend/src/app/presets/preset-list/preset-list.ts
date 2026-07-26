import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { PresetService, Preset, PresetExercise } from '../preset.service';
import { WorkoutService } from '../../workouts/workout.service';

@Component({
  selector: 'app-preset-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './preset-list.html'
})
export class PresetList implements OnInit {
  presets: Preset[] = [];
  loading = true;
  error = '';

  constructor(
    private presetService: PresetService,
    private workoutService: WorkoutService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadPresets();
  }

  loadPresets(): void {
    this.loading = true;
    this.error = '';
    this.presetService.getAll().subscribe({
      next: (presets) => {
        this.presets = presets;
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

  formatPresetExercise(pe: PresetExercise): string {
    if (pe.defaultDuration) {
      return `(${pe.defaultDuration}s)`;
    }
    const parts = [`${pe.defaultSets}x${pe.defaultReps}`];
    if (pe.defaultWeight) {
      parts.push(`@${pe.defaultWeight}kg`);
    }
    return `(${parts.join(' ')})`;
  }

  createWorkoutFromPreset(presetId: number): void {
    this.error = '';
    this.workoutService.createFromPreset(presetId).subscribe({
      next: (workout) => this.router.navigate(['/workouts', workout.id]),
      error: (err) => { this.error = this.extractError(err); this.cdr.markForCheck(); }
    });
  }

  deletePreset(id: number): void {
    if (confirm('Are you sure you want to delete this preset?')) {
      this.error = '';
      this.presetService.delete(id).subscribe({
        next: () => this.loadPresets(),
        error: (err) => { this.error = this.extractError(err); this.cdr.markForCheck(); }
      });
    }
  }

  private extractError(err: any): string {
    const body = err.error;
    if (typeof body === 'string') return body;
    if (body?.message) return body.message;
    if (Array.isArray(body)) return body.map((e: any) => e.description).join('. ');
    return 'Operation failed';
  }
}
