## Why

The codebase has accumulated technical debt from rapid feature development. The backend has fat controllers with inline DTOs and no service layer for most endpoints, leading to N+1 query issues and unmaintainable code. The frontend lacks consistent folder organization (no pages/ separation, flat progress/ folder, triple-nested settings path) and has interfaces scattered across service files. Additionally, several UX gaps remain: duration-based exercises lack unit selection, registration has no password confirmation, the UI is too small for comfortable use, and new users have no onboarding guidance.

## What Changes

### Backend Refactoring
- **BREAKING** Extract business logic from controllers into service classes (WorkoutsService, ExercisesService, PresetsService, StatsService, UserService)
- **BREAKING** Move all 18 inline DTOs into a dedicated `DTOs/` folder, split by request/response
- Fix 2 N+1 query hotspots in DashboardController (foreach+await loops)
- Add AutoMapper for entity↔DTO mapping instead of manual object initializers
- Replace anonymous object responses in StatsController with typed response DTOs

### Frontend Restructuring
- Create `pages/` folder for all route-level components
- Create `components/` folder for shared child components (chart-tile, chart-editor)
- Create `models/` folder with separate files for each domain model interface
- Flatten settings/ to remove triple-nested `settings/settings/settings` path
- Structure progress/ into subdirectories matching other features
- Remove unnecessary CommonModule imports (Angular 22 uses @if/@for natively)
- Fix 4 subscriptions with missing error handlers
- Export User interface (currently private to auth.service.ts)

### Feature: Duration Unit
- Add `DurationUnit` enum (seconds, minutes, hours) to Exercise model
- Add unit selector dropdown in exercise form (visible when isDuration checked)
- Display unit labels in workout/preset forms instead of hardcoded "(s)"
- Add "duration" metric type to chart system

### Feature: Password Confirmation
- Add ConfirmPassword field to RegisterRequest DTO
- Add validation (must match, min length) on both backend and frontend
- Add second password input to registration form

### Feature: UI Sizing
- Bump base font from 0.8rem to 0.875rem (14px)
- Increase nav height from 40px to 48px
- Bump heading and label sizes proportionally
- Keep max-width at 960px and TUI aesthetic

### Feature: Theme Default
- Change default theme from 'dark' to 'auto' (respects OS preference)

### Feature: Nav Cleanup
- Add "presets" link to navigation bar

### Feature: Onboarding Guide
- Create OnboardingGuide component shown on /workouts after first login
- Simple instruction box: Create Preset → Log Workout → Track Progress
- Dismiss permanently via localStorage flag
- Hidden once user creates their first workout

## Capabilities

### New Capabilities
- `backend-layering`: Service layer extraction, DTO separation, mapper integration
- `duration-units`: Duration unit enum, exercise form dropdown, unit-aware display, chart metric
- `onboarding`: First-time user guide component with dismissal

### Modified Capabilities
(none — no existing specs)

## Impact

### Backend
- `Controllers/` — all 7 controllers refactored to thin HTTP-only handlers
- `Services/` — 5 new services created
- `DTOs/` — new folder with ~20 DTO classes extracted from controllers
- `Data/GymDbContext.cs` — new DurationUnit field on Exercise, WorkoutExercise
- `Migrations/` — new migration for DurationUnit enum column
- `Program.cs` — register new services in DI

### Frontend
- `src/app/` — complete folder restructure (pages/, components/, models/)
- All route imports in `app.routes.ts` updated to new paths
- `styles.css` — font size and spacing bumps
- `app.ts` — default theme changed to 'auto'
- `app.html` — presets nav link added
- New component: `onboarding/`
- New models directory: `models/`
- 11 interfaces extracted from service files into models/

### Dependencies
- Add AutoMapper NuGet package to backend
- No new frontend dependencies (onboarding is custom, no tour library)
