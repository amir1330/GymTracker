## Context

The gym tracker codebase has accumulated dead code, structural inconsistencies, and an N+1 query bug since the initial cleanup. A team lead review identified specific issues that need fixing before the codebase is considered production-quality.

Current state:
- `StatsService` is registered in DI but never injected anywhere
- `WorkoutsController.CreateFromPreset` endpoint is never called by the frontend
- `DashboardService.GetAllChartDataAsync` has a `foreach` with `await` inside (N+1 query)
- `WorkoutExercise.RestTime` property exists but is never read or written
- `UserService._userManager` is injected but unused
- `CreateDashboardChartRequest` and `UpdateDashboardChartRequest` are identical
- `ChartDataPoint`/`ChartSummary` are defined in `ChartService` but referenced by DTOs
- `DashboardController.Create` response is missing `ExerciseName`
- `UserSettings.Theme` default mismatch between model ("dark") and AuthController ("auto")
- `UserSettings.User` nav property missing `[JsonIgnore]`
- All frontend subscriptions use `{ next: ..., error: ... }` pattern
- `pages/settings/settings.ts` has redundant naming
- `auth.model.ts` is empty
- `GymDbContext.cs` is in `Data/` but migrations are in `Migrations/`

## Goals / Non-Goals

**Goals:**
- Remove all dead code (services, endpoints, properties, files)
- Fix the N+1 query in dashboard chart loading
- Simplify frontend subscription patterns
- Clean up structural inconsistencies (naming, colocating, layer violations)
- Consolidate duplicate DTOs

**Non-Goals:**
- Performance optimization beyond the N+1 fix
- Adding new features
- Refactoring business logic or adding new capabilities
- Changing the database schema beyond removing orphaned columns
- Updating tests (no tests exist in this codebase)

## Decisions

### D1: Batch-load chart data instead of per-iteration
**Decision**: Load all workout data for all charts in a single query, then filter in-memory per chart.

**Why**: The current N+1 pattern runs a DB query per chart (could be up to 20 charts). A single query with the union of all periods/exercises, then in-memory filtering, eliminates the loop.

**Alternative considered**: Load all user workouts once and compute all metrics in memory. Rejected because different charts query different time periods and exercises, so a targeted query per chart is still needed — just batched.

### D2: Create AuthService for AuthController
**Decision**: Extract user creation, settings creation, and token generation from `AuthController` into a new `AuthService`.

**Why**: The team lead requires services for all controllers. Currently `AuthController` does everything inline.

**Alternative considered**: Keep inline but add comments. Rejected — the requirement is explicit about service layer separation.

### D3: Consolidate Create/Update DTOs into one
**Decision**: Merge `CreateDashboardChartRequest` and `UpdateDashboardChartRequest` into a single `DashboardChartRequest`.

**Why**: They are byte-for-byte identical. Maintaining two identical files is pointless duplication.

### D4: Move ChartDataPoint/ChartSummary to DTOs
**Decision**: Move `ChartDataPoint` and `ChartSummary` from `ChartService.cs` into `DTOs/Dashboard/`. Update `ChartService` and `DashboardChartData` to reference the DTO versions.

**Why**: DTOs should not depend on service-layer types. This eliminates the layer violation in `DashboardChartData.cs`.

### D5: Simple subscription pattern for all frontend
**Decision**: Replace all `{ next: ..., error: ... }` subscriptions with `.subscribe(data => ...)`.

**Why**: Team lead says this is premature optimization. The HTTP interceptor handles 401 globally. Other errors are rare and don't need per-component handlers.

**Risk**: If a component needs to show errors, we'll need to add error handling back. Mitigation: We can always add `.subscribe({ next: ..., error: ... })` back for specific cases where user-facing error display is needed.

### D6: Rename settings page file
**Decision**: Rename `pages/settings/settings.ts` to `pages/settings/account.ts` (and class `Settings` → `Account`).

**Why**: Avoids the "settings inside settings" naming issue. The component shows account info and theme settings, so `Account` is a better name.

### D7: Move Migrations into Data/
**Decision**: Move the `Migrations/` folder into `Data/` so it sits alongside `GymDbContext.cs`.

**Why**: Team lead requirement to colocate migrations and context.

## Risks / Trade-offs

- **[Breaking API]** `POST /api/workouts/from-preset/{presetId}` is removed → Frontend never calls it, so no impact. Verified by searching frontend code.
- **[Subscription simplification]** Removing error handlers means errors are silently swallowed → Acceptable for now. The global interceptor handles auth errors. If error display is needed later, add it back.
- **[Batch loading complexity]** Loading all chart data in one query then filtering in-memory is more complex → Necessary to eliminate N+1. The data set is bounded by user's workout history (typically <1000 rows).
- **[DB migration]** Dropping `RestTime` column is irreversible → Column is confirmed empty/unused across the entire codebase.
