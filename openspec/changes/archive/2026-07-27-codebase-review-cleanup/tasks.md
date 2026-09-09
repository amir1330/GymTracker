## 1. Backend Dead Code Removal

- [x] 1.1 Delete `Services/StatsService.cs` and remove `builder.Services.AddScoped<StatsService>();` from `Program.cs`
- [x] 1.2 Delete `WorkoutsController.CreateFromPreset` endpoint (lines 71-99) and remove `PresetsService` dependency from `WorkoutsController` constructor
- [x] 1.3 Remove `WorkoutExercise.RestTime` property from `Models/WorkoutExercise.cs`
- [x] 1.4 Create EF Core migration to drop `RestTime` column from `WorkoutExercises` table and apply it
- [x] 1.5 Remove unused `_userManager` field from `Services/UserService.cs`
- [x] 1.6 Remove unused `using System.ComponentModel.DataAnnotations` from `Models/WorkoutExercise.cs` and `Models/PresetExercise.cs`
- [x] 1.7 Add `[JsonIgnore]` to `UserSettings.User` navigation property

## 2. Backend N+1 Query Fix

- [x] 2.1 Rewrite `DashboardService.GetAllChartDataAsync` to batch-load workout data in a single query instead of per-iteration
- [x] 2.2 Verify chart data still loads correctly for multi-chart dashboards

## 3. Backend DTO Cleanup

- [x] 3.1 Merge `CreateDashboardChartRequest` and `UpdateDashboardChartRequest` into a single `DashboardChartRequest`
- [x] 3.2 Update `DashboardController.Create` and `DashboardController.Update` to use the consolidated DTO
- [x] 3.3 Move `ChartDataPoint` and `ChartSummary` from `ChartService.cs` into `DTOs/Dashboard/`
- [x] 3.4 Update `DashboardChartData` DTO to reference DTO-layer types instead of `ChartService` types
- [x] 3.5 Update `ChartService` to use the DTO-layer `ChartDataPoint` and `ChartSummary`
- [x] 3.6 Fix `DashboardController.Create` response to include `ExerciseName` (load Exercise navigation)

## 4. Backend Service Layer

- [x] 4.1 Create `Services/AuthService.cs` with methods: `RegisterAsync`, `LoginAsync`
- [x] 4.2 Move user creation, settings creation, and token generation logic from `AuthController` into `AuthService`
- [x] 4.3 Register `AuthService` in DI (`Program.cs`)
- [x] 4.4 Update `AuthController` to delegate to `AuthService`

## 5. Backend Model Consistency

- [x] 5.1 Change `UserSettings.Theme` default from `"dark"` to `"auto"` in the model class
- [x] 5.2 Remove explicit `Theme = "auto"` from `AuthController.Register` (now handled by model default)

## 6. Backend Colocation

- [x] 6.1 Move `Migrations/` folder contents into `Data/` (alongside `GymDbContext.cs`)

## 7. Frontend Subscription Simplification

- [x] 7.1 Replace `{ next: ..., error: ... }` with `.subscribe(data => ...)` in `pages/progress/progress-page.ts`
- [x] 7.2 Replace subscription pattern in `pages/exercise-form/exercise-form.ts`
- [x] 7.3 Replace subscription pattern in `pages/exercise-list/exercise-list.ts`
- [x] 7.4 Replace subscription pattern in `pages/preset-form/preset-form.ts`
- [x] 7.5 Replace subscription pattern in `pages/preset-list/preset-list.ts`
- [x] 7.6 Replace subscription pattern in `pages/workout-form/workout-form.ts`
- [x] 7.7 Replace subscription pattern in `pages/workout-list/workout-list.ts`
- [x] 7.8 Replace subscription pattern in `pages/login/login.ts`
- [x] 7.9 Replace subscription pattern in `pages/register/register.ts`
- [x] 7.10 Replace subscription pattern in `pages/settings/settings.ts`
- [x] 7.11 Replace subscription pattern in `components/chart-editor/chart-editor.ts`

## 8. Frontend Cleanup

- [x] 8.1 Delete empty `models/auth.model.ts`
- [x] 8.2 Rename `pages/settings/settings.ts` to `pages/settings/account.ts` and rename class `Settings` → `Account`
- [x] 8.3 Update `app.routes.ts` to reference the renamed component
- [x] 8.4 Update `app.html` if it references the settings component by name

## 9. Build Verification

- [x] 9.1 Verify backend builds clean (`dotnet build`)
- [x] 9.2 Verify frontend builds clean (`npx ng build`)
- [ ] 9.3 Run the application and verify settings page shows email correctly
- [ ] 9.4 Verify dashboard charts load without N+1 queries
