## 1. Remove Rest Timer

- [x] 1.1 Delete `rest-timer/` component files (ts, html, css)
- [x] 1.2 Remove `RestTimer` import from `workout-detail.ts` (component deleted)
- [x] 1.3 Remove timer-related settings from `settings.ts` and `settings.html`

## 2. Remove Workout Detail Page

- [x] 2.1 Delete `workout-detail/` component files (ts, html, css)
- [x] 2.2 Remove `/workouts/:id` route from `app.routes.ts`
- [x] 2.3 Remove any navigation links to `/workouts/:id` across the app

## 3. Update Navigation & Routes

- [x] 3.1 Change default route redirect from `/workouts` to `/progress` in `app.routes.ts`
- [x] 3.2 Update nav bar in `app.html`: 4 tabs — Progress, Log, Exercises, Settings
- [x] 3.3 Rename "Workouts" nav link to "Log" pointing to `/workouts`
- [x] 3.4 Make Progress the first nav item
- [x] 3.5 Add presets link/section to Settings page (since removed from nav)

## 4. Simplify Workout List (Log Page)

- [x] 4.1 Remove "View" button/link from workout cards in `workout-list.html`
- [x] 4.2 Show exercises inline in each workout card (name, sets × reps × weight, or duration)
- [x] 4.3 Add Edit button — navigate to workout form with workout ID
- [x] 4.4 Keep Delete button — inline delete with confirmation

## 5. Add Preset Loading to Workout Form

- [x] 5.1 Inject `PresetService` into `workout-form.ts`
- [x] 5.2 Load user's presets on form init
- [x] 5.3 Add preset buttons row above exercise list in `workout-form.html`
- [x] 5.4 Implement `loadPreset(preset)` — replaces exercise list with preset defaults
- [x] 5.5 Show "No presets yet" message when no presets exist
- [x] 5.6 Remove rest time field from exercise entries in the form

## 6. Remove Rest Time from Workout Form

- [x] 6.1 Remove rest time input from `workout-form.html` exercise entries
- [x] 6.2 Remove `restTime` field from workout exercise creation
- [x] 6.3 Verify backend still accepts requests without restTime (field is optional)

## 7. Cleanup & Verify

- [x] 7.1 Remove any dead imports (RestTimer, WorkoutDetail)
- [x] 7.2 Run `ng build` — verify zero errors
- [x] 7.3 Verify: login → redirected to /progress
- [x] 7.4 Verify: Log page shows workout cards with inline exercises
- [x] 7.5 Verify: New workout form shows preset buttons, loads exercises on click
- [x] 7.6 Verify: Edit workout form pre-fills correctly
- [x] 7.7 Verify: Delete workout works from list
- [x] 7.8 Verify: No rest timer anywhere in the app
- [x] 7.9 Fix chart editor radio buttons (missing name attributes)
- [x] 7.10 Verify: Chart editor closes after adding
