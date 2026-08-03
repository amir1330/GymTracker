import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { RouterModule } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { ExerciseService } from '../../services/exercise.service';
import { Exercise } from '../../models/exercise.model';
import { TranslationService } from '../../services/translation.service';

@Component({
  selector: 'app-exercise-list',
  standalone: true,
  imports: [RouterModule, TranslatePipe],
  templateUrl: './exercise-list.html',
  styleUrls: ['./exercise-list.css']
})
export class ExerciseList implements OnInit {
  exercises: Exercise[] = [];
  loading = true;

  constructor(
    private exerciseService: ExerciseService,
    private cdr: ChangeDetectorRef,
    private translationService: TranslationService
  ) {}

  ngOnInit(): void {
    this.loadExercises();
  }

  loadExercises(): void {
    this.loading = true;
    this.exerciseService.getAll().subscribe(exercises => {
      this.exercises = exercises;
      this.loading = false;
      this.cdr.markForCheck();
    });
  }

  deleteExercise(id: number): void {
    const message = this.translationService.instant('common.confirmDelete', { item: this.translationService.instant('exercise.title') });
    if (confirm(message)) {
      this.exerciseService.delete(id).subscribe(() => this.loadExercises());
    }
  }
}
