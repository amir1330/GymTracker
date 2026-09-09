## 1. Backend — Remove Dead Endpoints

- [x] 1.1 Remove `AddExercise`, `RemoveExercise`, `UpdateRestTime` stub endpoints from WorkoutsController
- [x] 1.2 Remove `GetStats`, `GetExerciseProgress`, `GetExercises` endpoints from StatsController
- [x] 1.3 Remove `UpdateProfile` endpoint from UserController
- [x] 1.4 Remove unused `IMapper` injection from PresetsController

## 2. Backend — Gut Dead Service Methods

- [x] 2.1 Remove `GetStatsAsync`, `GetExerciseProgressAsync`, `GetExerciseFrequencyAsync`, `GetWorkoutsForChartAsync` from StatsService (keep `ExerciseExistsAsync` only)
- [x] 2.2 Remove `GetByIdAsync` from DashboardService
- [x] 2.3 Remove `ExistsAsync` from ExercisesService
- [x] 2.4 Remove `UpdateProfileAsync`, `GetUserByIdAsync`, `GetSettingsAsync` from UserService

## 3. Backend — Delete Dead DTOs

- [x] 3.1 Delete `DTOs/Stats/StatsResponse.cs`
- [x] 3.2 Delete `DTOs/Stats/ExerciseStatsResponse.cs`
- [x] 3.3 Delete `DTOs/Stats/ExerciseProgressResponse.cs`
- [x] 3.4 Delete `DTOs/Workouts/UpdateRestTimeRequest.cs`
- [x] 3.5 Delete `DTOs/User/UpdateProfileRequest.cs`
- [x] 3.6 Move `ChartDataRequest` from `Services/ChartService.cs` to `DTOs/Stats/`
- [x] 3.7 Move `DashboardChartData` from `Services/DashboardService.cs` to `DTOs/Dashboard/`

## 4. Backend — Delete Dead AutoMapper Profiles

- [x] 4.1 Delete `Mappings/WorkoutProfile.cs`
- [x] 4.2 Delete `Mappings/PresetProfile.cs`
- [x] 4.3 Delete `Mappings/UserProfile.cs`
- [x] 4.4 Remove identity mapping `Exercise→Exercise` from ExerciseProfile

## 5. Backend — Clean Model Properties & DTOs

- [x] 5.1 Remove `Height` property from User model
- [x] 5.2 Remove `RestTimerEnabled` and `DefaultRestTimeSeconds` from UserSettings model
- [x] 5.3 Simplify `UpdateSettingsRequest` to only `Theme`
- [x] 5.4 Simplify `UserService.UpdateSettingsAsync` to only `theme` param
- [x] 5.5 Remove `RestTime` from `WorkoutExerciseRequest` DTO
- [x] 5.6 Remove dead `GET /api/user/profile` endpoint
- [x] 5.7 Clean `AuthController` registration defaults

## 6. Frontend — Remove Dead Code

- [x] 6.1 Remove `User` interface from `auth.model.ts` (emptied file)
- [x] 6.2 Remove `currentUserSubject`, `loadUserFromToken` from AuthService
- [x] 6.3 Remove `createFromPreset()` from WorkoutService
- [x] 6.4 Remove `weight` from Register component class
- [x] 6.5 Remove `restTime` from WorkoutExercise model
- [x] 6.6 Remove unused `Router` injection from WorkoutList
- [x] 6.7 Delete `user.model.ts` (UserProfile no longer used)
- [x] 6.8 Remove `getProfile()` from SettingsService
- [x] 6.9 Simplify Settings page to read email from JWT token

## 7. Frontend — Extract Shared Utility & Clean Imports

- [x] 7.1 Create `utils/error.util.ts` with shared `extractError()` function
- [x] 7.2 Replace 7 copy-pasted `extractError()` methods with import from shared utility
- [x] 7.3 Remove unnecessary `CommonModule` imports from 10 page components
- [x] 7.4 Remove duplicate `.app` CSS from `app.css` (deleted file)

## 8. Verification

- [x] 8.1 Backend compiles successfully
- [x] 8.2 Frontend compiles successfully
- [ ] 8.3 All existing API endpoints return correct responses (auth, exercises, workouts, presets, dashboard)
- [ ] 8.4 Frontend pages load and function correctly
