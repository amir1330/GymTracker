## Tech Stack

| Layer | Technology |
|-------|------------|
| Backend | ASP.NET Web API |
| Frontend | Angular |
| Database | PostgreSQL |

## Context

Building a full-stack gym tracking application with user authentication, exercise catalog, workout presets, and performance tracking. Users need to log in, manage exercises, create workout templates, and track their workouts with optional rest timer.

## Goals / Non-Goals

**Goals:**
- JWT authentication for user accounts
- Exercise catalog with pre-loaded common exercises
- Workout presets for quick workout creation
- Detailed workout logging with performance tracking
- Optional rest timer (toggleable in settings)
- Mobile-responsive Angular UI (works on phone-width screens)

**Non-Goals:**
- Cloud deployment (local PostgreSQL only)
- Advanced analytics or reporting
- Mobile native applications
- Social features or sharing

## Decisions

**Backend: ASP.NET Web API with JWT Auth**
- ASP.NET for strong typing and EF Core integration
- JWT tokens for stateless authentication
- ASP.NET Identity for user management

**Frontend: Angular**
- Comprehensive framework with routing, forms, HTTP client
- Custom CSS (no Angular Material) for Gruvbox theming
- Requires zone.js polyfill for change detection — without it, async HTTP responses never trigger view updates (the "Loading..." bug)

**UI Design:**
- Gruvbox color palette (light/dark modes with toggle)
- JetBrains Mono font throughout
- TUI-inspired aesthetic: sharp borders, minimal shadows, clean layout
- No emojis - text-only icons and labels
- Minimal, functional design - content over decoration

**Database: PostgreSQL with EF Core**
- Code-first migrations
- Seeded exercise data on first run

**Project Structure:**
```
gym-tracker/
├── backend/
│   ├── Controllers/
│   ├── Models/
│   ├── Data/
│   ├── Services/      # JWT, Auth
│   └── Migrations/
├── frontend/
│   └── src/app/
│       ├── auth/       # Login, Register
│       ├── exercises/
│       ├── presets/
│       ├── workouts/
│       ├── settings/
│       └── services/
└── docker-compose.yml
```

**API Endpoints:**
- `/api/auth/register`, `/api/auth/login` - Authentication
- `/api/exercises` - Exercise CRUD
- `/api/presets` - Workout preset CRUD
- `/api/workouts` - Workout log CRUD (incl. add/remove exercise, rest-time update)
- `/api/user/profile`, `/api/user/settings` - User profile and settings
- `/api/stats` - Aggregate stats, per-exercise progress, exercise breakdown

**Data Model:**
- User: Id, Username, Email, Weight, Height, Settings (JSON)
- Exercise: Id, Name, MuscleGroup, IsDuration, IsDefault
- Preset: Id, Name, UserId, PresetExercises[]
- PresetExercise: PresetId, ExerciseId, DefaultSets, DefaultReps, DefaultWeight, DefaultDuration
- Workout: Id, UserId, Date, Notes, BodyWeight, WorkoutExercises[]
- WorkoutExercise: WorkoutId, ExerciseId, Sets, Reps, Weight, Duration, RestTime

## Risks / Trade-offs

- [Risk] JWT secret management → Mitigation: Store in appsettings.json, rotate periodically
- [Risk] Seeded data conflicts → Mitigation: Check for existing data before seeding
- [Risk] Complex preset → workout flow → Mitigation: Simple copy with edit UI
