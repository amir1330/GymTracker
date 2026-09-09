## Context

The gym tracker codebase has completed all features and is in a stable state. A comprehensive audit revealed accumulated dead code across backend and frontend layers. This cleanup is a pure maintenance task with zero new feature work.

**Current state:**
- 7 dead API endpoints (3 are no-ops in WorkoutsController, 3 in StatsController, 1 in UserController)
- ~60% of StatsController/StatsService is unused
- 3 of 4 AutoMapper profiles entirely dead
- 7 copy-pasted `extractError()` functions across frontend components
- 12 files with unnecessary `CommonModule` imports
- Multiple unused model properties and interfaces

## Goals / Non-Goals

**Goals:**
- Remove all dead API endpoints that are never called by the frontend
- Remove dead service methods, DTOs, and AutoMapper profiles
- Remove unused model properties from both backend and frontend
- Extract `extractError()` into a shared utility
- Remove unnecessary `CommonModule` imports
- Remove duplicate CSS
- Keep all existing functionality working identically

**Non-Goals:**
- Refactoring working code or changing architecture
- Adding new features or endpoints
- Changing database schema (EF migrations not needed — removing unused properties only)
- Changing test coverage (no tests exist in this project)
- Performance optimization beyond removing dead code paths

## Decisions

### D1: Delete no-op endpoints entirely vs. implement them

**Decision:** Delete the 3 no-op WorkoutsController endpoints (`AddExercise`, `RemoveExercise`, `UpdateRestTime`).

**Rationale:** These were stubs that never called `SaveChangesAsync()`. No frontend code calls them. Implementing them would add unneeded complexity. Deleting is correct.

### D2: Keep StatsController alive vs. remove it entirely

**Decision:** Keep StatsController with only `ExerciseExistsAsync` endpoint, remove 3 dead endpoints.

**Rationale:** `ExerciseExistsAsync` is called by the frontend to validate exercise selection. The other 3 endpoints (`GetStats`, `GetExerciseProgress`, `GetExercises`) are never called. Gutting the controller is better than removing it entirely since it still has one useful endpoint.

### D3: Keep AutoMapper profiles vs. remove entirely

**Decision:** Keep only ExerciseProfile with its 2 live mappings. Delete WorkoutProfile, PresetProfile, UserProfile entirely.

**Rationale:** WorkoutProfile and PresetProfile were replaced by manual LINQ projections in WorkoutService and PresetsService (for N+1 optimization). UserProfile was for the removed UpdateProfile endpoint. ExerciseProfile still has 2 active mappings used by ExercisesService.

### D4: Extract shared extractError vs. leave as-is

**Decision:** Extract to a shared utility in `utils/error.util.ts`.

**Rationale:** 7 identical copies is clear technical debt. A single utility reduces duplication and makes future error handling changes easier.

### D5: Remove CommonModule imports vs. leave them

**Decision:** Remove from 12 files. Keep in chart-editor and chart-tile (they use ng2-charts directives that may need it).

**Rationale:** Angular 22 uses `@if`/`@for`/`@switch` natively — `CommonModule` is not needed for those. ng2-charts components may depend on it for template directives.

### D6: EF migration for removing model properties

**Decision:** No migration needed. Removing unused properties from C# models does not affect the database until a new migration is explicitly created.

**Rationale:** EF Core only creates migrations when `dotnet ef migrations add` is run. Removing properties from models is safe without a migration — the columns stay in the database but are simply ignored.

## Risks / Trade-offs

- **[Risk] Missing a consumer of a dead endpoint** → Mitigated by comprehensive frontend service audit (grep confirmed zero calls)
- **[Risk] AutoMapper profile removal breaks something** → Mitigated by checking all 3 deleted profiles have zero mappings used in any service
- **[Risk] Removing User.Height property causes runtime errors** → Mitigated by checking frontend never sends or reads height after settings removal
- **[Trade-off] StatsController kept with 1 endpoint** → Slightly less clean than removing entirely, but preserves a useful validation endpoint
- **[Trade-off] CommonModule kept in chart components** → Conservative choice; could be removed but risk of breaking ng2-charts is low benefit
