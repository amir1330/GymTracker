import { Exercise } from './exercise.model';

export interface PresetExercise {
  id?: number;
  exerciseId: number;
  exercise?: Exercise;
  defaultSets: number;
  defaultReps: number;
  defaultWeight?: number;
  defaultDuration?: number;
}

export interface Preset {
  id: number;
  name: string;
  presetExercises: PresetExercise[];
}
