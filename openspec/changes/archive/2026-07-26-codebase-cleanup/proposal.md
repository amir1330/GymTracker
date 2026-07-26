## Why

The codebase has accumulated dead code from removed features (rest-timer, stats page), duplicate DTOs, unused packages, and orphaned files. This cleanup removes noise, reduces maintenance surface, and makes the codebase easier to navigate.

## What Changes

- Delete dead `StatsPage` component + `StatsService` (never routed)
- Delete 3 dead methods from `WorkoutService` (`addExercise`, `removeExercise`, `updateRestTime`)
- Delete orphaned files (`GymTracker.http`, `backend.log`)
- Remove 11 orphaned rest-timer CSS blocks from `styles.css`
- Remove 3 orphaned CSS classes (`.exercise-name`, `.exercise-stats`, `.rest-time`)
- Remove dead interface fields from `SettingsService` (`restTimerEnabled`, `defaultRestTimeSeconds`)
- Merge duplicate DTOs (`AddWorkoutExerciseRequest` → `WorkoutExerciseRequest`, `CreateExerciseRequest` ≡ `UpdateExerciseRequest`)
- Remove unused NuGet package `Microsoft.AspNetCore.OpenApi`
- Remove unused `using` statements in `User.cs` and `UserSettings.cs`
- Extract duplicated chart computation from `StatsController` + `DashboardController` into shared `ChartService`
- Delete 8 empty CSS files or remove their `styleUrl` references

## Capabilities

### New Capabilities

- `chart-service`: Shared chart computation logic extracted from StatsController and DashboardController

### Modified Capabilities

(none — this is a cleanup, no behavior changes)

## Impact

- Backend: `WorkoutsController.cs`, `ExercisesController.cs`, `StatsController.cs`, `DashboardController.cs`, `GymTracker.csproj`, `Models/User.cs`, `Models/UserSettings.cs`
- Frontend: `stats/` directory (delete), `workout.service.ts`, `settings.service.ts`, `styles.css`, 8 component CSS files
- No API changes, no database changes, no breaking changes
