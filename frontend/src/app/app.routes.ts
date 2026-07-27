import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/progress', pathMatch: 'full' },
  { path: 'login', loadComponent: () => import('./pages/login/login').then(m => m.Login) },
  { path: 'register', loadComponent: () => import('./pages/register/register').then(m => m.Register) },
  { path: 'workouts', loadComponent: () => import('./pages/workout-list/workout-list').then(m => m.WorkoutList), canActivate: [authGuard] },
  { path: 'workouts/new', loadComponent: () => import('./pages/workout-form/workout-form').then(m => m.WorkoutForm), canActivate: [authGuard] },
  { path: 'workouts/:id/edit', loadComponent: () => import('./pages/workout-form/workout-form').then(m => m.WorkoutForm), canActivate: [authGuard] },
  { path: 'presets', loadComponent: () => import('./pages/preset-list/preset-list').then(m => m.PresetList), canActivate: [authGuard] },
  { path: 'presets/new', loadComponent: () => import('./pages/preset-form/preset-form').then(m => m.PresetForm), canActivate: [authGuard] },
  { path: 'presets/:id/edit', loadComponent: () => import('./pages/preset-form/preset-form').then(m => m.PresetForm), canActivate: [authGuard] },
  { path: 'exercises', loadComponent: () => import('./pages/exercise-list/exercise-list').then(m => m.ExerciseList), canActivate: [authGuard] },
  { path: 'exercises/new', loadComponent: () => import('./pages/exercise-form/exercise-form').then(m => m.ExerciseForm), canActivate: [authGuard] },
  { path: 'exercises/:id/edit', loadComponent: () => import('./pages/exercise-form/exercise-form').then(m => m.ExerciseForm), canActivate: [authGuard] },
  { path: 'progress', loadComponent: () => import('./pages/progress/progress-page').then(m => m.ProgressPage), canActivate: [authGuard] },
  { path: 'settings', loadComponent: () => import('./pages/settings/settings').then(m => m.Settings), canActivate: [authGuard] },
  { path: '**', redirectTo: '/progress' }
];
