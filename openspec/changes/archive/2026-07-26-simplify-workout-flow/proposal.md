## Why

The current app conflates two workflows: logging past workouts and tracking active workouts with rest timers. The user needs a simple gym logging app — record what they did, see progress over time. Rest timers, workout detail pages, and fragmented preset usage add unnecessary complexity. The Unix philosophy applies: do one thing well.

## What Changes

- **BREAKING** Remove rest timer component and all timer-related UI
- **BREAKING** Remove workout detail page (`/workouts/:id` route)
- **BREAKING** Change default landing page from `/workouts` to `/progress` (charts dashboard)
- Add "Load from Preset" button in workout form that populates exercise list with preset defaults
- Simplify workout list to show exercises inline with edit/delete (no detail navigation)
- Simplify navigation to 4 items: Progress, Log, Exercises, Settings
- Remove timer-related settings from Settings page

## Capabilities

### New Capabilities
- `preset-loading`: Loading preset templates directly from the workout creation form

### Modified Capabilities
- `workout-management`: Simplified — remove detail view, remove rest timer, add preset loading to form
- `navigation`: Reduced to 4 tabs, progress is default landing

## Impact

- **Frontend components removed**: `workout-detail/`, `rest-timer/`
- **Frontend components modified**: `workout-form/` (add preset loading), `workout-list/` (inline display, no detail link), `app.html` (nav), `app.routes.ts` (routes), `settings/` (remove timer toggle)
- **Backend**: No API changes needed — existing endpoints support the simplified flow
- **Breaking**: Any bookmarks to `/workouts/:id` will 404; rest timer settings removed
