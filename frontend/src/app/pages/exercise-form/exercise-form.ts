import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { ExerciseService } from '../../services/exercise.service';
import { Exercise } from '../../models/exercise.model';

@Component({
  selector: 'app-exercise-form',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './exercise-form.html'
})
export class ExerciseForm implements OnInit {
  exercise: Partial<Exercise> = {
    name: '',
    muscleGroup: '',
    isDuration: false,
    durationUnit: 'seconds'
  };
  muscleGroups = ['Chest', 'Back', 'Shoulders', 'Legs', 'Arms', 'Core', 'Cardio'];
  isEdit = false;
  error = '';

  constructor(
    private exerciseService: ExerciseService,
    private router: Router,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEdit = true;
      this.exerciseService.getById(+id).subscribe({
        next: (exercise) => { this.exercise = exercise; this.cdr.markForCheck(); },
        error: () => this.router.navigate(['/exercises'])
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

  onSubmit(): void {
    this.error = '';
    if (this.isEdit && this.exercise.id) {
      this.exerciseService.update(this.exercise.id, this.exercise).subscribe({
        next: () => this.router.navigate(['/exercises']),
        error: (err) => { this.error = this.extractError(err); this.cdr.markForCheck(); }
      });
    } else {
      this.exerciseService.create(this.exercise).subscribe({
        next: () => this.router.navigate(['/exercises']),
        error: (err) => { this.error = this.extractError(err); this.cdr.markForCheck(); }
      });
    }
  }
}
