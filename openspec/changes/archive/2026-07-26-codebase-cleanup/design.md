## Context

The gym tracker app was built iteratively through multiple changes (gym-tracker, dashboard-progress-charts, simplify-workout-flow). Features were added and removed but cleanup was never done. Result: dead code from removed features (rest-timer, stats page), duplicate DTOs, unused packages, and orphaned files.

## Goals / Non-Goals

**Goals:**
- Remove all dead code (unused components, methods, CSS, files)
- Eliminate duplicate DTOs
- Extract duplicated chart computation into shared service
- Reduce maintenance surface

**Non-Goals:**
- Refactoring working code
- Changing API contracts or behavior
- Database migrations
- Adding new features

## Decisions

### 1. Delete entire `stats/` directory

The `StatsPage` component and `StatsService` were built but never routed to. The `DashboardController` + progress charts replaced this functionality. No other component references `StatsService`. Safe to delete entirely.

**Alternative considered:** Keep as dead code for potential future use → Rejected: dead code rots. Git history preserves it if needed.

### 2. Extract chart computation into `ChartService`

`StatsController` and `DashboardController` both contain identical `GetCutoffDate()`, near-identical `ComputePoints()`, and near-identical `ComputeSummary()`. The Dashboard version uses `List<object>` and reflection — fragile and untyped.

**Decision:** Extract into `backend/Services/ChartService.cs` using the typed models from `StatsController`. Both controllers delegate to it.

**Alternative considered:** Just delete `StatsController` → Rejected: its endpoints (`/api/stats`, `/api/stats/exercise/{id}/progress`, `/api/stats/exercises`) are still called by `StatsService`... wait, `StatsService` is dead. But `StatsController` endpoints are called by the frontend? Let me verify.

Actually — the frontend `StatsService` is dead (only used by dead `StatsPage`). But the `StatsController` endpoints are still live and could be called. Need to verify frontend usage before deleting.

**Revised decision:** Keep `StatsController` endpoints alive but extract shared logic. Both controllers use `ChartService`.

### 3. Merge duplicate DTOs

`AddWorkoutExerciseRequest` and `WorkoutExerciseRequest` are character-for-character identical. `CreateExerciseRequest` and `UpdateExerciseRequest` are identical.

**Decision:** Use one DTO for each. In `WorkoutsController`, replace `AddWorkoutExerciseRequest` with `WorkoutExerciseRequest`. In `ExercisesController`, use `ExerciseRequest` for both create and update.

### 4. Delete empty CSS files

8 component CSS files are empty but referenced via `styleUrl`. Angular 22 handles this fine — the empty file just adds overhead.

**Decision:** Remove the `styleUrl` reference from the component decorator AND delete the empty `.css` file. Cleaner than keeping empty files.

### 5. Remove orphaned CSS from `styles.css`

11 CSS blocks for rest-timer and 3 other orphaned classes. No templates reference them.

**Decision:** Delete them. Git history preserves if needed.

## Risks / Trade-offs

- **Risk:** Deleting `stats/` directory might break something if there's an indirect reference → **Mitigation:** Grep confirmed no references outside the directory itself
- **Risk:** Extracting `ChartService` might introduce regressions in chart computation → **Mitigation:** Existing tests (if any) + manual verification. The logic is identical, just extracted.
- **Risk:** Merging DTOs might subtly change API validation → **Mitigation:** DTOs are identical, no validation differences
