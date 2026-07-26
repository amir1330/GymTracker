## 1. Project Setup

- [x] 1.1 Create backend/ directory and initialize ASP.NET Web API project
- [x] 1.2 Create frontend/ directory and initialize Angular project (no UI framework - custom CSS)
- [x] 1.3 Set up PostgreSQL connection string in appsettings.json
- [x] 1.4 Create docker-compose.yml for local PostgreSQL

## 2. Database Layer

- [x] 2.1 Create Entity models: User, Exercise, Preset, PresetExercise, Workout, WorkoutExercise, UserSettings
- [x] 2.2 Create GymDbContext with DbSets for all models
- [x] 2.3 Configure Entity relationships and constraints
- [x] 2.4 Create and apply initial EF Core migration
- [x] 2.5 Create seed data for default exercises (Bench Press, Squat, Deadlift, etc.)

## 3. Authentication

- [x] 3.1 Install JWT and Identity packages
- [x] 3.2 Configure JWT authentication in Program.cs
- [x] 3.3 Create AuthController with register and login endpoints
- [x] 3.4 Create JWT service for token generation
- [x] 3.5 Add [Authorize] attributes to protected endpoints

## 4. User Profile

- [x] 4.1 Create UserController with GET/PUT profile endpoints
- [x] 4.2 Add weight and height fields to User model
- [x] 4.3 Create user settings endpoint for timer toggle

## 5. Exercise Catalog

- [x] 5.1 Create ExercisesController with CRUD endpoints
- [x] 5.2 Add IsDuration and IsDefault flags to Exercise model
- [x] 5.3 Add validation: prevent modify/delete of default exercises
- [x] 5.4 Add duplicate name check on create

## 6. Workout Presets

- [x] 6.1 Create PresetsController with CRUD endpoints
- [x] 6.2 Create PresetExercise model and relationships
- [x] 6.3 Add endpoint to create workout from preset

## 7. Workout Logs

- [x] 7.1 Create WorkoutsController with CRUD endpoints
- [x] 7.2 Add workout exercise tracking (sets, reps, weight, duration, restTime)
- [x] 7.3 Add endpoint to add/remove exercises from workout
- [x] 7.4 Add workout creation from preset with value copying

## 8. Frontend - Auth

- [x] 8.1 Create login and register components
- [x] 8.2 Create auth service with JWT storage
- [x] 8.3 Add auth interceptor for API calls
- [x] 8.4 Add route guards for protected pages

## 9. Frontend - Exercises

- [x] 9.1 Create exercise list component
- [x] 9.2 Create exercise form for add/edit
- [x] 9.3 Add delete with confirmation

## 10. Frontend - Presets

- [x] 10.1 Create preset list component
- [x] 10.2 Create preset form with exercise selection
- [x] 10.3 Add "Create Workout" action from preset

## 11. Frontend - Workouts

- [x] 11.1 Create workout list sorted by date
- [x] 11.2 Create workout detail with exercise list
- [x] 11.3 Create workout form for add/edit
- [x] 11.4 Add exercise performance editing (sets, reps, weight)

## 12. Frontend - Settings

- [x] 12.1 Create settings page with profile edit
- [x] 12.2 Add rest timer toggle
- [x] 12.3 Add default rest time setting

## 13. Frontend - Rest Timer

- [x] 13.1 Create rest timer component
- [x] 13.2 Add timer display in workout detail
- [x] 13.3 Store rest time in workout exercises

## 14. Theming

- [x] 14.1 Create Gruvbox color variables (light and dark themes)
- [x] 14.2 Create theme service for toggling light/dark mode
- [x] 14.3 Set up JetBrains Mono font import
- [x] 14.4 Create base TUI-style components (borders, inputs, buttons)
- [x] 14.5 Add theme toggle to settings page
- [x] 14.6 Persist theme preference in user settings

## 15. Integration

- [x] 15.1 Set up Angular proxy config for API calls
- [x] 15.2 Configure CORS in backend
- [x] 15.3 Test full flow: register → create exercise → create preset → log workout

## 16. Post-Implementation Fixes

- [x] 16.1 Add zone.js polyfill and ChangeDetectorRef.markForCheck() to all components (root cause of "Loading..." bug)
- [x] 16.2 Add mobile-responsive CSS: @media breakpoints for nav, forms, exercise-entries, tables
- [x] 16.3 Create stats/progress page with exercise frequency, muscle group, calendar, per-exercise charts
- [x] 16.4 Add exercise delete check for preset associations (not just workouts)
- [x] 16.5 Add restTime persistence endpoint and rest-timer elapsed-time recording
- [x] 16.6 Fix login/register button styling (missing .btn class)
- [x] 16.7 Add 401 handling in auth interceptor (logout + redirect on token expiry)
