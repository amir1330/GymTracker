## 1. Backend: DTO Extraction

- [x] 1.1 Create `DTOs/` folder structure with domain subdirectories (Auth/, Workouts/, Exercises/, Presets/, Stats/, Dashboard/, User/)
- [x] 1.2 Extract Auth DTOs: RegisterRequest, LoginRequest from AuthController.cs
- [x] 1.3 Extract Workout DTOs: CreateWorkoutRequest, UpdateWorkoutRequest, WorkoutExerciseRequest, UpdateRestTimeRequest from WorkoutsController.cs
- [x] 1.4 Extract Exercise DTO: ExerciseRequest from ExercisesController.cs
- [x] 1.5 Extract Preset DTOs: CreatePresetRequest, UpdatePresetRequest, PresetExerciseRequest from PresetsController.cs
- [x] 1.6 Extract User DTOs: UpdateProfileRequest, UpdateSettingsRequest from UserController.cs
- [x] 1.7 Extract Dashboard DTOs: CreateDashboardChartRequest, UpdateDashboardChartRequest, ReorderRequest from DashboardController.cs
- [x] 1.8 Create typed response DTOs: StatsResponse, ChartDataResponse, ChartSummaryResponse, ExerciseResponse, PresetResponse, WorkoutResponse (replace anonymous objects)
- [x] 1.9 Update all controller files to reference extracted DTOs

## 2. Backend: Service Layer

- [x] 2.1 Install AutoMapper NuGet package
- [x] 2.2 Create AutoMapper profiles: WorkoutProfile, ExerciseProfile, PresetProfile, StatsProfile, DashboardProfile, UserProfile
- [x] 2.3 Register AutoMapper in Program.cs
- [x] 2.4 Create ExercisesService with CRUD operations, register in DI
- [x] 2.5 Create PresetsService with CRUD operations, register in DI
- [x] 2.6 Create WorkoutsService with CRUD operations + SyncProfileWeight, register in DI
- [x] 2.7 Create StatsService with statistics queries, register in DI
- [x] 2.8 Create DashboardService with chart CRUD + reorder, register in DI
- [x] 2.9 Create UserService with profile/settings updates, register in DI

## 3. Backend: Controller Refactoring

- [x] 3.1 Refactor ExercisesController to use ExercisesService + AutoMapper (remove _context injection)
- [x] 3.2 Refactor PresetsController to use PresetsService + AutoMapper (remove _context injection)
- [x] 3.3 Refactor WorkoutsController to use WorkoutsService + AutoMapper (remove _context injection)
- [x] 3.4 Refactor StatsController to use StatsService + typed DTOs (remove anonymous objects)
- [x] 3.5 Refactor DashboardController to use DashboardService + AutoMapper (remove _context injection)
- [x] 3.6 Refactor UserController to use UserService + AutoMapper (remove _context injection)

## 4. Backend: N+1 Query Fixes

- [x] 4.1 Fix DashboardController.GetAll() — batch-load chart data instead of foreach+await loop
- [x] 4.2 Fix DashboardController.Reorder() — load all charts in single WHERE IN query instead of loop

## 5. Backend: Duration Unit

- [x] 5.1 Create DurationUnit enum (Seconds, Minutes, Hours)
- [x] 5.2 Add DurationUnit field to Exercise model with default Seconds
- [x] 5.3 Add DurationUnit field to WorkoutExercise model (inherited from Exercise)
- [x] 5.4 Create EF Core migration for DurationUnit columns
- [x] 5.5 Update ExerciseRequest DTO to include DurationUnit
- [x] 5.6 Update ExercisesService to handle DurationUnit
- [x] 5.7 Add duration metric to ChartService (normalize to seconds for comparison)

## 6. Backend: Password Confirmation

- [x] 6.1 Add ConfirmPassword field to RegisterRequest DTO
- [x] 6.2 Add server-side validation: must match Password, minimum length

## 7. Backend: Build Verification

- [x] 7.1 Run `dotnet build` and fix any compilation errors
- [x] 7.2 Run `dotnet ef migrations add` for DurationUnit migration
- [x] 7.3 Verify backend starts and API responds

## 8. Frontend: Model Extraction

- [x] 8.1 Create `models/` directory
- [x] 8.2 Create `models/exercise.model.ts` — extract Exercise interface from exercise.service.ts
- [x] 8.3 Create `models/workout.model.ts` — extract Workout, WorkoutExercise interfaces from workout.service.ts
- [x] 8.4 Create `models/preset.model.ts` — extract Preset, PresetExercise interfaces from preset.service.ts
- [x] 8.5 Create `models/dashboard.model.ts` — extract ChartPoint, ChartSummary, ChartDataResponse, DashboardChart from dashboard.service.ts
- [x] 8.6 Create `models/user.model.ts` — extract UserProfile from settings.service.ts
- [x] 8.7 Create `models/auth.model.ts` — extract User interface from auth.service.ts (currently private, make it exported)
- [x] 8.8 Update all service files to import from models/ instead of inline definitions

## 9. Frontend: Folder Restructure

- [x] 9.1 Create `pages/` directory and move route-level components (login, register, workout-list, workout-form, preset-list, preset-form, exercise-list, exercise-form, progress, settings)
- [x] 9.2 Create `components/` directory and move shared child components (chart-tile, chart-editor)
- [x] 9.3 Create `services/` directory and move all service files
- [x] 9.4 Flatten settings/ — move settings-page component to `pages/settings/`, remove nested settings/settings/ folder
- [x] 9.5 Structure progress/ — move progress-page to `pages/progress/`, chart-tile and chart-editor to `components/`
- [x] 9.6 Update all imports in app.routes.ts to new paths
- [x] 9.7 Update all component imports to new paths
- [x] 9.8 Remove unnecessary CommonModule imports (Angular 22 uses @if/@for natively)

## 10. Frontend: Duration Unit UI

- [x] 10.1 Add DurationUnit to Exercise interface in models/exercise.model.ts
- [x] 10.2 Add DurationUnit to WorkoutExercise interface in models/workout.model.ts
- [x] 10.3 Update exercise form: show unit dropdown when isDuration is checked
- [x] 10.4 Update workout form: show unit label next to duration input
- [x] 10.5 Update preset form: show unit label next to duration input
- [x] 10.6 Update workout-list display: format duration with unit suffix (30min, 1.5hr, 45s)
- [x] 10.7 Update preset-list display: format duration with unit suffix
- [x] 10.8 Add DurationUnit to chart-editor metric options

## 11. Frontend: Password Confirmation

- [x] 11.1 Add confirmPassword field to register form template (second password input)
- [x] 11.2 Add confirmPassword validation (must match, minimum length)
- [x] 11.3 Update register component to send confirmPassword in request

## 12. Frontend: UI Sizing

- [x] 12.1 Bump body font-size from 0.8rem to 0.875rem
- [x] 12.2 Bump nav height from 40px to 48px
- [x] 12.3 Bump heading sizes (h2: 1rem→1.1rem, h3: 0.8rem→0.9rem)
- [x] 12.4 Bump form labels from 0.75rem to 0.8rem
- [x] 12.5 Bump badge/tag fonts from 0.7rem to 0.75rem

## 13. Frontend: Theme & Nav

- [x] 13.1 Change default theme in app.ts from 'dark' to 'auto'
- [x] 13.2 Add "presets" link to nav bar in app.html

## 14. Frontend: Onboarding

- [x] 14.1 Create onboarding/ directory with onboarding-guide component (.ts, .html, .css)
- [x] 14.2 Implement 3-step instruction layout (Create Preset, Log Workout, Track Progress)
- [x] 14.3 Add localStorage check: show guide if onboardingDismissed is not set
- [x] 14.4 Add "Got it" button that sets localStorage flag and hides guide
- [x] 14.5 Style guide using Gruvbox theme variables (TUI aesthetic)

## 15. Frontend: Build Verification

- [x] 15.1 Run `ng build` and fix any compilation errors
- [x] 15.2 Verify all routes load correctly
- [x] 15.3 Verify theme toggle works (auto/dark/light)
- [x] 15.4 Verify onboarding guide appears on first visit and dismisses correctly

## 16. Git

- [x] 16.1 Stage all changes
- [x] 16.2 Commit with descriptive message
- [ ] 16.3 Push to GitHub
