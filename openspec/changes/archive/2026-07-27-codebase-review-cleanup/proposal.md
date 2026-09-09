## Why

Team lead review identified structural and quality issues across the codebase: N+1 query in a loop, dead code, inconsistent layer separation, premature error handling patterns in the frontend, and folder naming issues. These need to be addressed before the codebase is considered clean.

## What Changes

### Backend
- **BREAKING**: Remove dead `StatsService` (entire class + DI registration)
- **BREAKING**: Remove dead `WorkoutsController.CreateFromPreset` endpoint (never called by frontend)
- Remove orphaned `WorkoutExercise.RestTime` property and DB column
- Remove unused `_userManager` from `UserService`
- Fix N+1 query in `DashboardService.GetAllChartDataAsync` — batch DB queries instead of per-iteration
- Add `AuthController` → `AuthService` service layer (currently all logic is inline in the controller)
- Consolidate identical `CreateDashboardChartRequest` / `UpdateDashboardChartRequest` into one DTO
- Move `ChartDataPoint` / `ChartSummary` out of `ChartService` into DTOs (layer violation)
- Add `DashboardChartData` internal types to avoid DTO → Service layer dependency
- Fix `DashboardController.Create` response missing `ExerciseName`
- Fix `UserSettings.Theme` default mismatch (model="dark" vs AuthController="auto")
- Add `[JsonIgnore]` to `UserSettings.User` navigation property
- Remove unused `using` statements in `WorkoutExercise.cs` and `PresetExercise.cs`
- Move `GymDbContext.cs` next to Migrations (or Migrations into `Data/`)

### Frontend
- Remove all `{ next: ..., error: ... }` subscription patterns — use simple `.subscribe(data => ...)` instead
- Rename `pages/settings/settings.ts` to avoid settings/settings naming (e.g., `account.ts`)
- Delete empty `auth.model.ts`

## Capabilities

### New Capabilities
- `backend-layer-cleanup`: Service layer consistency, DTO consolidation, dead code removal, N+1 fix
- `frontend-subscription-simplify`: Remove premature error handling patterns, simplify subscriptions

### Modified Capabilities

## Impact

- **Backend controllers**: `AuthController`, `WorkoutsController`, `DashboardController`, `StatsController`
- **Backend services**: `DashboardService`, `StatsService` (deleted), `UserService`, `ChartService`
- **Backend models**: `WorkoutExercise`, `UserSettings`
- **Backend DTOs**: `Dashboard/` folder consolidation
- **Backend migration**: New migration to drop `RestTime` column
- **Frontend pages**: All page components (subscription pattern change)
- **Frontend models**: `auth.model.ts` deleted, `settings` renamed
- **API**: `POST /api/workouts/from-preset/{presetId}` removed (breaking)
