## Tech Stack

| Layer | Technology |
|-------|------------|
| Backend | ASP.NET Web API |
| Frontend | Angular |
| Database | PostgreSQL |

## Why

Need a personal gym tracking application to log workouts, track exercises, and monitor progress over time. Users need authentication, customizable exercise catalog, workout templates for quick logging, and performance tracking with optional rest timer.

## What Changes

- Full-stack web application: ASP.NET backend, Angular frontend, PostgreSQL database
- JWT authentication for user accounts
- Exercise catalog with pre-loaded common exercises and user-created custom exercises
- Workout presets (templates) for quick workout creation
- Workout logs tracking sets, reps, weight, duration, and rest time
- Optional rest timer in settings

## Capabilities

### New Capabilities

- `user-auth`: JWT-based registration and login with user profile (weight, height)
- `exercise-catalog`: Manage exercises (name, muscle group, optional duration flag) with pre-loaded defaults
- `workout-presets`: Create workout templates with exercises and default values for quick logging
- `workout-logs`: Track workouts from presets or scratch, recording performance per exercise
- `rest-timer`: Optional built-in timer for tracking rest between sets (toggleable in settings)
- `theming`: Gruvbox color palette (light/dark), JetBrains Mono font, TUI-inspired minimal aesthetic
- `progress-stats`: View workout frequency, muscle group distribution, calendar, and per-exercise progress over time

### Modified Capabilities

None - this is a new application.

## Impact

- New project structure with backend/ and frontend/ directories
- New dependencies: Angular, ASP.NET Web API, Entity Framework Core, Npgsql, JWT auth libraries
- Local PostgreSQL database required
- Pre-seeded exercise data in database
