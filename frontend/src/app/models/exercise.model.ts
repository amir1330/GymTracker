export type DurationUnit = 'seconds' | 'minutes' | 'hours';

export interface Exercise {
  id: number;
  name: string;
  muscleGroup: string;
  isDuration: boolean;
  durationUnit: DurationUnit;
  isDefault: boolean;
}
