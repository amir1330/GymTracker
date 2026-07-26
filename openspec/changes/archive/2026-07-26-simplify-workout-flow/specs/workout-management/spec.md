## MODIFIED Requirements

### Requirement: Workout creation form
The workout form SHALL support creating and editing workouts. The form fields are: Date (defaults to today), Notes (optional), Body Weight (optional), Exercises (dynamic list with add/remove).

**Changes from current:**
- REMOVE: Rest time field per exercise
- ADD: Preset loading buttons at top of form

**Scenarios:**
- **When** the user opens the form for editing an existing workout
  - **Then** all fields are pre-filled with the workout's current data
  - **And** exercises show their current sets/reps/weight values
- **When** the user saves a workout
  - **Then** the workout is created or updated via API
  - **And** the user is redirected to the Log page

### Requirement: Workout list display
The workout list SHALL display all workouts as cards with exercises shown inline. Each card shows: date, notes (if any), body weight (if any), exercise summary, edit and delete buttons.

**Changes from current:**
- REMOVE: "View" link to detail page
- KEEP: Edit button (opens workout form pre-filled)
- KEEP: Delete button (with confirmation)

**Scenarios:**
- **When** the user views the Log page
  - **Then** workouts are listed in reverse chronological order
  - **And** each workout card shows exercises inline (name, sets × reps × weight or duration)
- **When** the user clicks Edit on a workout
  - **Then** the workout form opens pre-filled with that workout's data
- **When** the user clicks Delete on a workout
  - **Then** a confirmation prompt appears
  - **And** on confirm, the workout is deleted and removed from the list

### Requirement: Workout detail page removed
The workout detail page (`/workouts/:id`) SHALL be removed. All functionality is covered by the list view (display) and form (edit).

**Scenarios:**
- **When** a user navigates to `/workouts/:id`
  - **Then** they are redirected to the Log page

### Requirement: Rest timer removed
The rest timer component and all timer-related UI SHALL be removed from the application.

**Scenarios:**
- **When** the user views a workout (in list or form)
  - **Then** no rest timer controls are shown
- **When** the user views Settings
  - **Then** no timer-related options are shown
