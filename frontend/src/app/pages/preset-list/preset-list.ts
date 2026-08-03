import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { RouterModule } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { PresetService } from '../../services/preset.service';
import { Preset, PresetExercise } from '../../models/preset.model';
import { TranslationService } from '../../services/translation.service';

@Component({
  selector: 'app-preset-list',
  standalone: true,
  imports: [RouterModule, TranslatePipe],
  templateUrl: './preset-list.html'
})
export class PresetList implements OnInit {
  presets: Preset[] = [];
  loading = true;

  constructor(
    private presetService: PresetService,
    private cdr: ChangeDetectorRef,
    private translationService: TranslationService
  ) {}

  ngOnInit(): void {
    this.loadPresets();
  }

  loadPresets(): void {
    this.loading = true;
    this.presetService.getAll().subscribe(presets => {
      this.presets = presets;
      this.loading = false;
      this.cdr.markForCheck();
    });
  }

  formatPresetExercise(pe: PresetExercise): string {
    if (pe.defaultDuration) {
      const unit = pe.exercise?.durationUnit || 'seconds';
      const short = unit === 'seconds' ? 's' : unit === 'minutes' ? 'min' : 'hr';
      return `(${pe.defaultDuration}${short})`;
    }
    const parts = [`${pe.defaultSets}x${pe.defaultReps}`];
    if (pe.defaultWeight) {
      parts.push(`@${pe.defaultWeight}${this.translationService.instant('common.kg')}`);
    }
    return `(${parts.join(' ')})`;
  }

  deletePreset(id: number): void {
    const message = this.translationService.instant('common.confirmDelete', { item: this.translationService.instant('preset.title') });
    if (confirm(message)) {
      this.presetService.delete(id).subscribe(() => this.loadPresets());
    }
  }
}
