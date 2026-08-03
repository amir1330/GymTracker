import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { ExerciseService } from '../../services/exercise.service';
import { Exercise } from '../../models/exercise.model';

@Component({
  selector: 'app-exercise-form',
  standalone: true,
  imports: [FormsModule, RouterModule, TranslatePipe],
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
      this.exerciseService.getById(+id).subscribe(exercise => {
        this.exercise = exercise;
        this.cdr.markForCheck();
      });
    }
  }

  onSubmit(): void {
    if (this.isEdit && this.exercise.id) {
      this.exerciseService.update(this.exercise.id, this.exercise).subscribe(() => this.router.navigate(['/exercises']));
    } else {
      this.exerciseService.create(this.exercise).subscribe(() => this.router.navigate(['/exercises']));
    }
  }
}
