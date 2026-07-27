import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { RouterModule } from '@angular/router';
import { ExerciseService } from '../../services/exercise.service';
import { Exercise } from '../../models/exercise.model';

@Component({
  selector: 'app-exercise-list',
  standalone: true,
  imports: [RouterModule],
  templateUrl: './exercise-list.html',
  styleUrls: ['./exercise-list.css']
})
export class ExerciseList implements OnInit {
  exercises: Exercise[] = [];
  loading = true;

  constructor(private exerciseService: ExerciseService, private cdr: ChangeDetectorRef) {}

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
    if (confirm('Are you sure you want to delete this exercise?')) {
      this.exerciseService.delete(id).subscribe(() => this.loadExercises());
    }
  }
}
