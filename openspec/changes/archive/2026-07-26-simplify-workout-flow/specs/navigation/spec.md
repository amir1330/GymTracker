## MODIFIED Requirements

### Requirement: Navigation structure
The app navigation SHALL show 4 tabs: Progress, Log, Exercises, Settings.

**Changes from current:**
- REMOVE: "Presets" as a top-level nav item (presets accessible via Settings or workout form)
- RENAME: "Workouts" → "Log"
- REORDER: Progress is first (default after login)

**Scenarios:**
- **When** the user logs in
  - **Then** they are redirected to `/progress` (charts dashboard)
- **When** the user clicks "Progress"
  - **Then** the progress/charts dashboard is displayed
- **When** the user clicks "Log"
  - **Then** the workout list is displayed
- **When** the user clicks "Exercises"
  - **Then** the exercise catalog is displayed
- **When** the user clicks "Settings"
  - **Then** the settings page is displayed
  - **And** presets are accessible from Settings (link or section)

### Requirement: Default route
The root path `/` SHALL redirect to `/progress`.

**Scenarios:**
- **When** the user navigates to `/`
  - **Then** they are redirected to `/progress`
