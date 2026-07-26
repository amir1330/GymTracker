import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ExerciseService, Exercise } from '../exercise.service';

@Component({
  selector: 'app-exercise-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './exercise-list.html',
  styleUrls: ['./exercise-list.css']
})
export class ExerciseList implements OnInit {
  exercises: Exercise[] = [];
  loading = true;
  error = '';

  constructor(private exerciseService: ExerciseService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.loadExercises();
  }

  loadExercises(): void {
    this.loading = true;
    this.error = '';
    this.exerciseService.getAll().subscribe({
      next: (exercises) => {
        this.exercises = exercises;
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

  deleteExercise(id: number): void {
    if (confirm('Are you sure you want to delete this exercise?')) {
      this.error = '';
      this.exerciseService.delete(id).subscribe({
        next: () => this.loadExercises(),
        error: (err) => {
          this.error = this.extractError(err);
          this.cdr.markForCheck();
        }
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
