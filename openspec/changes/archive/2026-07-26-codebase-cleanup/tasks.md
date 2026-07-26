## 1. Delete Dead Code (Frontend)

- [x] 1.1 Delete entire `frontend/src/app/stats/` directory (stats-page.ts, stats-page.html, stats-page.css, stats.service.ts)
- [x] 1.2 Remove 3 dead methods from `workout.service.ts` (addExercise, removeExercise, updateRestTime)
- [x] 1.3 Remove dead interface fields from `settings.service.ts` (restTimerEnabled, defaultRestTimeSeconds)
- [x] 1.4 Remove dead `currentUser$` and `User` interface export from `auth.service.ts`

## 2. Delete Dead Code (Backend)

- [x] 2.1 Delete `backend/GymTracker.http` (orphaned template file)
- [x] 2.2 Delete `backend/backend.log` (committed log file)
- [x] 2.3 Remove unused `using System.ComponentModel.DataAnnotations` from `Models/User.cs`
- [x] 2.4 Remove unused `using System.ComponentModel.DataAnnotations` from `Models/UserSettings.cs`
- [x] 2.5 Remove unused NuGet package `Microsoft.AspNetCore.OpenApi` from `GymTracker.csproj`

## 3. Remove Orphaned CSS

- [x] 3.1 Remove 8 rest-timer CSS blocks from `styles.css` (.rest-timer, .timer-display, .timer-display.warning, .timer-complete p, .timer-controls, .timer-overlay, and their mobile overrides)
- [x] 3.2 Remove 3 orphaned CSS classes from `styles.css` (.exercise-name, .exercise-stats, .rest-time)

## 4. Remove Empty CSS Files

- [x] 4.1 Remove `styleUrl` and delete empty CSS files: login.css, register.css, exercise-form.css, preset-form.css, preset-list.css, settings.css, workout-form.css, workout-list.css

## 5. Merge Duplicate DTOs

- [x] 5.1 In `WorkoutsController.cs`: replace `AddWorkoutExerciseRequest` with `WorkoutExerciseRequest` (remove duplicate class)
- [x] 5.2 In `ExercisesController.cs`: merge `CreateExerciseRequest` and `UpdateExerciseRequest` into single `ExerciseRequest`

## 6. Extract ChartService

- [x] 6.1 Create `backend/Services/ChartService.cs` with GetCutoffDate, ComputePoints, ComputeSummary methods
- [x] 6.2 Move `ChartDataPoint` and `ChartSummary` models to `Models/` (or keep in ChartService if preferred)
- [x] 6.3 Refactor `StatsController` to inject and use `ChartService`
- [x] 6.4 Refactor `DashboardController` to inject and use `ChartService` (removes reflection-based code)
- [x] 6.5 Register `ChartService` in `Program.cs`

## 7. Verify

- [x] 7.1 Build backend — zero errors, zero warnings
- [x] 7.2 Build frontend — zero errors
- [x] 7.3 Run backend, verify all API endpoints still work
- [x] 7.4 Manual test: login, exercises, workouts, progress charts, settings
