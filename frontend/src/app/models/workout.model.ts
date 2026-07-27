import { Exercise } from './exercise.model';
import { DurationUnit } from './exercise.model';

export interface WorkoutExercise {
  id?: number;
  exerciseId: number;
  exercise?: Exercise;
  sets: number;
  reps: number;
  weight?: number;
  duration?: number;
  durationUnit?: DurationUnit;
}

export interface Workout {
  id: number;
  date: string;
  notes?: string;
  bodyWeight?: number;
  workoutExercises: WorkoutExercise[];
}
