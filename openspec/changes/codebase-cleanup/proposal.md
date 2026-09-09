## Why

The codebase has accumulated significant dead code from rapid feature development and refactoring. A comprehensive audit found 7 dead API endpoints (3 are no-ops that never save to DB), ~60% of StatsController/StatsService is unused, 3 of 4 AutoMapper profiles are entirely dead, multiple unused model properties, and copy-pasted utility functions across 7 frontend components. This dead weight makes the codebase harder to navigate, increases bundle size, and creates confusion about what's actually used.

## What Changes

### Backend Cleanup
- **BREAKING** Remove 3 no-op stub endpoints from WorkoutsController (`AddExercise`, `RemoveExercise`, `UpdateRestTime`) that never persisted data
- **BREAKING** Remove 3 dead endpoints from StatsController (`GET /api/stats`, `GET /api/stats/exercise/{id}/progress`, `GET /api/stats/exercises`)
- **BREAKING** Remove `PUT /api/user/profile` endpoint (weight/height removed)
- Gut StatsService — keep only `ExerciseExistsAsync`, remove 4 dead methods
- Remove unused `GetByIdAsync` from DashboardService, `ExistsAsync` from ExercisesService
- Delete dead DTOs: `StatsResponse`, `ExerciseStatsResponse`, `ExerciseProgressResponse`, `UpdateRestTimeRequest`
- Delete dead AutoMapper profiles: `WorkoutProfile`, `PresetProfile`, `UserProfile` (entirely unused)
- Remove identity mapping `Exercise→Exercise` from ExerciseProfile
- Remove unused `IMapper` injection from PresetsController
- Clean model properties: remove `User.Height`, `UserSettings.RestTimerEnabled`, `UserSettings.DefaultRestTimeSeconds`
- Move `ChartDataRequest` from ChartService.cs to DTOs folder
- Move `DashboardChartData` from DashboardService.cs to DTOs folder

### Frontend Cleanup
- Remove dead `User` interface from `auth.model.ts` (entire interface unused)
- Remove dead `currentUserSubject` from AuthService
- Remove unused `createFromPreset()` from WorkoutService
- Remove unused `weight` property from Register component
- Remove unused `restTime` from WorkoutExercise model
- Remove unused `Router` injection from WorkoutList
- Extract shared `extractError()` utility (7 copy-pasted copies → 1 shared function)
- Remove unnecessary `CommonModule` imports from 12 files
- Remove duplicate `.app` CSS from `app.css` (already in `styles.css`)
- Remove unused `UserProfile.id` property

## Capabilities

### New Capabilities

### Modified Capabilities

## Impact

### Backend
- `Controllers/` — WorkoutsController loses 3 methods, StatsController loses 3 methods, UserController loses 1 method, PresetsController loses unused injection
- `Services/` — StatsService gutted, DashboardService/ExercisesService lose unused methods
- `DTOs/` — 4 files deleted, 2 classes moved from service files
- `Mappings/` — 3 files deleted, 1 file cleaned
- `Models/` — 3 properties removed from User, UserSettings

### Frontend
- `models/` — `auth.model.ts` User interface removed, `workout.model.ts` restTime removed, `user.model.ts` id removed
- `services/` — AuthService dead state removed, WorkoutService dead method removed
- `pages/` — 12 files lose CommonModule import, WorkoutList loses Router, Register loses weight
- `components/` — chart-editor/chart-tile keep CommonModule (they use ng2-charts directives)
- New shared utility: `utils/error.util.ts`
- `app.css` deleted

### Dependencies
- No new dependencies
- AutoMapper package stays (ExerciseProfile still uses it)
