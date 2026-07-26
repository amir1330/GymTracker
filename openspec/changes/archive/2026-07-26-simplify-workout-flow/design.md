## Context

The gym tracker app currently has two parallel workout creation flows (manual form vs preset quick-start), a workout detail page with rest timer, and the workout list as the landing page. The user wants a simpler logging-focused app where progress charts are the main screen and presets are loaded inline during workout creation.

Current state:
- Frontend: Angular 22, standalone components, zoneless change detection
- Backend: ASP.NET 8, PostgreSQL, EF Core
- All existing API endpoints remain unchanged

## Goals / Non-Goals

**Goals:**
- Progress charts as default landing page after login
- Unified workout form with inline preset loading
- Remove rest timer and workout detail page entirely
- 4-tab navigation: Progress, Log, Exercises, Settings
- Workouts list shows exercises inline (no detail page)

**Non-Goals:**
- No backend API changes needed
- No new database models
- No changes to exercise catalog or preset management
- No changes to progress charts or dashboard

## Decisions

### 1. Preset loading mechanism
**Decision:** Add a row of preset buttons at the top of the workout form. Clicking a preset populates the exercises array with preset defaults. User edits numbers as needed.

**Why not a modal/separate page:** Unix philosophy — minimal UI, one screen, no unnecessary navigation.

### 2. Workout detail page removal
**Decision:** Delete `workout-detail/` component entirely. Workout list shows exercises inline. Edit opens the same form pre-filled.

**Why:** Without rest timer, the detail page has no unique functionality. The list view + edit form covers everything.

### 3. Default route
**Decision:** Change `''` redirect from `/workouts` to `/progress`. Add `provideRouter` with `withComponentInputBinding()` if needed.

### 4. Navigation structure
**Decision:** 4 tabs in nav bar: Progress (default), Log, Exercises, Settings. Presets accessible from Settings or via the button in workout form.

### 5. Rest timer removal
**Decision:** Delete `rest-timer/` component. Remove timer settings from Settings page. Remove timer-related imports from any parent components.

## Risks

- **Bookmark breakage**: Any saved links to `/workouts/:id` will 404. Low risk for single-user app.
- **Preset loading UX**: If presets have many exercises, the form could feel crowded. Mitigated by scrollable exercise list (already exists).
