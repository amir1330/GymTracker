import { Routes } from '@angular/router';
import { authGuard } from './auth/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/progress', pathMatch: 'full' },
  { path: 'login', loadComponent: () => import('./auth/login/login').then(m => m.Login) },
  { path: 'register', loadComponent: () => import('./auth/register/register').then(m => m.Register) },
  { path: 'workouts', loadComponent: () => import('./workouts/workout-list/workout-list').then(m => m.WorkoutList), canActivate: [authGuard] },
  { path: 'workouts/new', loadComponent: () => import('./workouts/workout-form/workout-form').then(m => m.WorkoutForm), canActivate: [authGuard] },
  { path: 'workouts/:id/edit', loadComponent: () => import('./workouts/workout-form/workout-form').then(m => m.WorkoutForm), canActivate: [authGuard] },
  { path: 'presets', loadComponent: () => import('./presets/preset-list/preset-list').then(m => m.PresetList), canActivate: [authGuard] },
  { path: 'presets/new', loadComponent: () => import('./presets/preset-form/preset-form').then(m => m.PresetForm), canActivate: [authGuard] },
  { path: 'presets/:id/edit', loadComponent: () => import('./presets/preset-form/preset-form').then(m => m.PresetForm), canActivate: [authGuard] },
  { path: 'exercises', loadComponent: () => import('./exercises/exercise-list/exercise-list').then(m => m.ExerciseList), canActivate: [authGuard] },
  { path: 'exercises/new', loadComponent: () => import('./exercises/exercise-form/exercise-form').then(m => m.ExerciseForm), canActivate: [authGuard] },
  { path: 'exercises/:id/edit', loadComponent: () => import('./exercises/exercise-form/exercise-form').then(m => m.ExerciseForm), canActivate: [authGuard] },
  { path: 'progress', loadComponent: () => import('./progress/progress-page').then(m => m.ProgressPage), canActivate: [authGuard] },
  { path: 'settings', loadComponent: () => import('./settings/settings/settings').then(m => m.Settings), canActivate: [authGuard] },
  { path: '**', redirectTo: '/progress' }
];
