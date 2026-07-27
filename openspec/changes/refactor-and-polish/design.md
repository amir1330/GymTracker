## Context

The gym tracker app is a small full-stack project (ASP.NET 8 + Angular 22 + PostgreSQL) built over a few days of rapid prototyping. It works functionally but has accumulated structural debt:

- **Backend**: 5 of 7 controllers are "fat" — business logic, DB access, and DTO definitions all live in controller files. Two N+1 query hotspots exist in DashboardController. No mapper library is used.
- **Frontend**: 40 files across a mostly-flat structure. No separation between page-level and child components. Interfaces live in service files. The settings folder has an awkward triple-nested path.
- **UX gaps**: Duration exercises assume seconds with no unit selection, registration has no password confirmation, UI is cramped at 0.8rem, new users get no guidance.

The codebase is small enough that a single comprehensive refactor is feasible without risk of prolonged instability.

## Goals / Non-Goals

**Goals:**
- Establish proper controller → service → repository layering on the backend
- Extract all DTOs into a dedicated `DTOs/` folder with request/response separation
- Add AutoMapper to eliminate manual mapping boilerplate
- Fix both N+1 query issues in DashboardController
- Reorganize frontend into `pages/`, `components/`, `models/` structure
- Extract all 11 interfaces into `models/` with one interface per file
- Add DurationUnit enum with per-exercise configuration
- Add password confirmation to registration
- Improve UI sizing while preserving TUI aesthetic
- Default theme to auto (OS preference)
- Add presets to nav bar
- Create simple onboarding guide for new users

**Non-Goals:**
- Adding a full test suite (not requested)
- Implementing i18n/localization
- Adding new chart types beyond duration metric
- Real-time features (WebSocket, live updates)
- Mobile responsive overhaul (current mobile CSS is sufficient)
- Database schema changes beyond DurationUnit

## Decisions

### D1: Backend Layering Pattern

**Decision**: Controller → Service → EF Core DbContext (no repository layer)

**Rationale**: For a project this size, a repository layer adds indirection without value. Services will own business logic and DB access via DbContext directly. This is the standard ASP.NET pattern for small-to-medium projects.

**Alternatives considered**:
- Repository + Unit of Work: Overkill for direct EF Core usage. EF Core's DbContext already implements UoW pattern.
- CQRS/MediatR: Too heavy for 7 controllers with simple CRUD.

### D2: DTO Organization

**Decision**: `DTOs/` folder at project root, split by domain (Auth/, Workouts/, Exercises/, Presets/, Stats/, Dashboard/, User/)

**Rationale**: Keeps DTOs discoverable and avoids the inline-class anti-pattern. Domain grouping prevents a flat folder with 20+ files.

**Structure**:
```
DTOs/
├── Auth/
│   ├── RegisterRequest.cs
│   └── LoginRequest.cs
├── Workouts/
│   ├── CreateWorkoutRequest.cs
│   ├── UpdateWorkoutRequest.cs
│   ├── WorkoutExerciseRequest.cs
│   └── WorkoutResponse.cs
├── Exercises/
│   ├── ExerciseRequest.cs
│   └── ExerciseResponse.cs
├── Presets/
│   ├── CreatePresetRequest.cs
│   ├── UpdatePresetRequest.cs
│   ├── PresetExerciseRequest.cs
│   └── PresetResponse.cs
├── Stats/
│   └── StatsResponse.cs
├── Dashboard/
│   ├── CreateDashboardChartRequest.cs
│   ├── UpdateDashboardChartRequest.cs
│   ├── ReorderRequest.cs
│   ├── ChartDataResponse.cs
│   └── ChartSummaryResponse.cs
└── User/
    ├── UpdateProfileRequest.cs
    └── UpdateSettingsRequest.cs
```

### D3: Mapper Strategy

**Decision**: Add AutoMapper with profile classes per domain

**Rationale**: 18 DTOs with manual mapping is tedious and error-prone. AutoMapper's `Profile` + `CreateMap` is the standard .NET approach and reduces boilerplate significantly.

**Alternatives considered**:
- Manual mapping with extension methods: Less boilerplate than inline, but still repetitive for 18+ mappings.
- Mapster: Lighter but less common in .NET ecosystem. Team familiarity with AutoMapper is higher.

### D4: N+1 Fix Strategy

**Decision**: Batch-load chart data in DashboardController.GetAll() instead of per-chart

**Approach**:
1. Load all charts in one query
2. Collect all chart IDs and metadata
3. Batch-fetch workout data for all charts in a single query with过滤
4. Compute chart data in memory per chart

For Reorder(): Load all charts to be reordered in one query (`WHERE Id IN @ids`), then update in memory, then single SaveChanges.

### D5: Frontend Folder Structure

**Decision**: Feature-based with pages/components separation

```
src/app/
├── pages/           ← route-level components
│   ├── login/
│   ├── register/
│   ├── workout-list/
│   ├── workout-form/
│   ├── preset-list/
│   ├── preset-form/
│   ├── exercise-list/
│   ├── exercise-form/
│   ├── progress/
│   └── settings/
├── components/      ← shared child components
│   ├── chart-tile/
│   └── chart-editor/
├── models/          ← interfaces
│   ├── exercise.model.ts
│   ├── workout.model.ts
│   ├── preset.model.ts
│   ├── dashboard.model.ts
│   ├── user.model.ts
│   └── auth.model.ts
├── services/        ← HTTP services
│   ├── exercise.service.ts
│   ├── workout.service.ts
│   ├── preset.service.ts
│   ├── dashboard.service.ts
│   ├── settings.service.ts
│   └── auth.service.ts
├── guards/
├── interceptors/
├── onboarding/      ← new feature
│   └── onboarding-guide.*
├── app.*
├── app.config.ts
└── app.routes.ts
```

**Rationale**: Angular's official style guide recommends feature-based folders. Separating pages from child components makes it clear which components are routed. The current flat progress/ folder is inconsistent with how exercises/ and workouts/ are structured.

**Alternatives considered**:
- Keep current structure, just fix inconsistencies: Cleaner diff but doesn't address the team lead's feedback about pages/.
- Full lazy-loading per feature: Each page already lazy-loads via `loadComponent`. Adding lazy-loaded feature modules would be overkill for standalone components.

### D6: Duration Unit Storage

**Decision**: Add `DurationUnit` enum column to Exercise model, inherit to WorkoutExercise

**Approach**:
- New enum: `DurationUnit { Seconds, Minutes, Hours }`
- Exercise model gets `DurationUnit` (default: Seconds)
- WorkoutExercise gets `DurationUnit` (copied from Exercise at log time)
- PresetExercise does NOT get a unit field — it inherits from the linked Exercise
- Charts add a `duration` metric that normalizes to seconds for comparison

**Rationale**: Per-exercise default means users configure once. Inheriting at log time means if they change the exercise's unit later, old logs keep their original unit.

### D7: Onboarding Approach

**Decision**: Simple custom component, no tour library

**Approach**:
- Component shown on `/workouts` page when `localStorage.getItem('onboardingDismissed')` is null
- 3-step instruction box with clear call-to-action buttons linking to relevant pages
- "Got it" button sets localStorage flag permanently
- No spotlight/highlight overlay — just a styled instruction card

**Rationale**: A tour library (shepherd.js, intro.js) would add ~50KB for a feature that's seen once. A simple styled box is sufficient for a TUI-style app and matches the aesthetic.

### D8: UI Sizing Strategy

**Decision**: Bump root font-size to 0.875rem, scale everything proportionally

**Changes**:
- `body` font-size: 0.8rem → 0.875rem
- `h2`: 1rem → 1.1rem
- `h3`: 0.8rem → 0.9rem
- Labels: 0.75rem → 0.8rem
- Nav height: 40px → 48px
- Form inputs: 0.8rem → 0.875rem
- Badges: 0.7rem → 0.75rem

**Rationale**: ~10% increase keeps the compact TUI feel while improving readability. Mobile breakpoint remains at 640px.

## Risks / Trade-offs

- **[Large refactor diff]** → Mitigation: Do backend and frontend as separate commits. Each can be verified independently.
- **[AutoMapper learning curve]** → Mitigation: Simple Profile-based config, well-documented. Team lead specifically requested mappers.
- **[Migration for DurationUnit]** → Mitigation: Additive column with default value. No data loss. Backward compatible.
- **[Breaking folder structure]** → Mitigation: Update all imports in app.routes.ts. Angular compiler will catch any missed imports at build time.
- **[Onboarding localStorage only]** → Mitigation: Sufficient for single-user app. If user clears localStorage, they see the guide again — acceptable.
