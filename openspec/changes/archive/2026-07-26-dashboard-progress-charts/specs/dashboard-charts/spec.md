## ADDED Requirements

### Requirement: User can add chart tiles to dashboard
The system SHALL allow authenticated users to add configurable chart tiles to their personal progress dashboard. Each tile SHALL display a chart with a label, metric, exercise (when applicable), time period, and chart type.

#### Scenario: Add a chart tile
- **WHEN** user clicks "Add Chart" on the progress page and submits a valid configuration (label, metric, period, chart type, and exercise if required)
- **THEN** a new chart tile is created in the database and appears on the dashboard

#### Scenario: Add chart without exercise when metric requires it
- **WHEN** user submits a chart configuration with metric "weight" and no exercise selected
- **THEN** the system SHALL reject the request with a validation error

### Requirement: Chart metrics
The system SHALL support the following metrics: `weight` (heaviest set per session), `volume` (sets × reps × weight), `est1rm` (estimated 1RM via Epley formula: weight × (1 + reps/30)), `reps` (total reps per session), `bodyWeight` (workout body weight), `frequency` (workouts per week).

#### Scenario: Weight metric
- **WHEN** metric is "weight" and exercise is "Bench Press" and period is "30d"
- **THEN** the system SHALL return one data point per workout containing Bench Press, with value equal to the heaviest weight used in that workout

#### Scenario: Volume metric with all exercises
- **WHEN** metric is "volume" and exercise is null (all) and period is "90d"
- **THEN** the system SHALL return one data point per workout with value equal to the sum of (sets × reps × weight) across all exercises in that workout

#### Scenario: Volume metric with specific exercise
- **WHEN** metric is "volume" and exercise is "Bench Press" and period is "30d"
- **THEN** the system SHALL return one data point per workout containing Bench Press, with value equal to sets × reps × weight for that exercise

#### Scenario: Estimated 1RM metric
- **WHEN** metric is "est1rm" and exercise is "Squat" and period is "90d"
- **THEN** the system SHALL return one data point per workout containing Squat, with value equal to the highest estimated 1RM across all sets (max of weight × (1 + reps/30))

#### Scenario: Reps metric
- **WHEN** metric is "reps" and exercise is "Bench Press" and period is "30d"
- **THEN** the system SHALL return one data point per workout containing Bench Press, with value equal to the sum of reps across all sets

#### Scenario: Body weight metric
- **WHEN** metric is "bodyWeight" and period is "30d"
- **THEN** the system SHALL return one data point per workout that has a bodyWeight value, with value equal to the bodyWeight

#### Scenario: Frequency metric
- **WHEN** metric is "frequency" and period is "90d"
- **THEN** the system SHALL return one data point per week with value equal to the number of workouts in that week

### Requirement: Rolling time periods
The system SHALL support the following rolling periods: `7d` (last 7 days), `30d` (last 30 days), `90d` (last 90 days), `180d` (last 180 days), `365d` (last 365 days), `all` (all time). All periods SHALL be computed relative to the current date.

#### Scenario: Rolling 30-day period
- **WHEN** today is July 25, 2026 and period is "30d"
- **THEN** the system SHALL include data from June 25, 2026 to July 25, 2026

#### Scenario: All time period
- **WHEN** period is "all"
- **THEN** the system SHALL include all workout data for the user regardless of date

### Requirement: Chart types
The system SHALL support two chart types: `line` (line chart with points) and `bar` (bar chart).

#### Scenario: Line chart rendering
- **WHEN** chart type is "line"
- **THEN** the chart SHALL render as a connected line with data points, with a smooth tension curve

#### Scenario: Bar chart rendering
- **WHEN** chart type is "bar"
- **THEN** the chart SHALL render as vertical bars for each data point

### Requirement: Chart data response
The system SHALL return chart data as an array of `{date, value}` points plus a summary object with `current` (latest value), `best` (maximum value), `change` (percentage change string), and `trend` ("up", "down", or "flat").

#### Scenario: Chart data with summary
- **WHEN** a chart has data points with values [70, 72.5, 75]
- **THEN** the response SHALL include `current: 75`, `best: 75`, `change: "+7.1%"`, `trend: "up"`

#### Scenario: Chart with no data
- **WHEN** a chart configuration matches no workout data (e.g., metric "weight" for an exercise never performed)
- **THEN** the system SHALL return an empty points array and summary values of null

### Requirement: Live preview
The system SHALL show a live chart preview in the add/edit modal that updates as the user changes configuration options. The preview SHALL use the same chart rendering as the dashboard.

#### Scenario: Preview updates on metric change
- **WHEN** user changes the metric selector in the chart editor modal
- **THEN** the preview chart SHALL update to show data for the new metric within 500ms

#### Scenario: Preview shows empty state
- **WHEN** the selected configuration has no matching data
- **THEN** the preview SHALL display a "No data for this configuration" message

### Requirement: Dashboard persistence
Chart tile configurations SHALL be persisted per user in a `DashboardCharts` table. Each tile SHALL store: label, metric, exerciseId (nullable), period, chartType, and position (integer for ordering).

#### Scenario: Dashboard loads saved charts
- **WHEN** user navigates to /progress
- **THEN** the system SHALL load all saved chart configurations for the user and render them with current data

#### Scenario: Dashboard empty state
- **WHEN** user has no saved chart tiles
- **THEN** the system SHALL display a "No charts yet" message with a prompt to add one

### Requirement: Edit chart tile
The system SHALL allow users to edit an existing chart tile's configuration. All fields (label, metric, exercise, period, chartType) SHALL be editable.

#### Scenario: Edit chart configuration
- **WHEN** user clicks edit on a chart tile, changes metric from "weight" to "volume", and saves
- **THEN** the chart tile SHALL update its configuration and re-render with volume data

### Requirement: Delete chart tile
The system SHALL allow users to delete a chart tile from the dashboard.

#### Scenario: Delete chart
- **WHEN** user clicks delete on a chart tile and confirms
- **THEN** the chart tile SHALL be removed from the dashboard and the database

### Requirement: Reorder chart tiles
The system SHALL allow users to reorder chart tiles using up/down controls. The position SHALL be persisted.

#### Scenario: Move chart up
- **WHEN** user clicks the up arrow on a chart tile that is not first
- **THEN** the tile SHALL swap positions with the tile above it and the new order SHALL be persisted

#### Scenario: Move first chart up
- **WHEN** user clicks the up arrow on the first chart tile
- **THEN** the button SHALL be disabled or hidden (no action)

### Requirement: Chart exercise filtering
The exercise selector SHALL be context-sensitive to the selected metric. For `bodyWeight` and `frequency` metrics, the exercise selector SHALL be hidden. For `weight`, `est1rm`, and `reps`, the exercise selector SHALL be required and only show individual exercises. For `volume`, the exercise selector SHALL be optional with an "All exercises" option.

#### Scenario: Body weight hides exercise selector
- **WHEN** user selects metric "bodyWeight"
- **THEN** the exercise selector SHALL be hidden from the form

#### Scenario: Volume shows all exercises option
- **WHEN** user selects metric "volume"
- **THEN** the exercise selector SHALL show "All exercises" as the first option

### Requirement: Dashboard API endpoint
The system SHALL provide a `GET /api/dashboard` endpoint that returns all chart tiles for the authenticated user with their computed chart data in a single response. The system SHALL also provide `POST /api/dashboard`, `PUT /api/dashboard/{id}`, `DELETE /api/dashboard/{id}`, and `PUT /api/dashboard/reorder` endpoints.

#### Scenario: Get dashboard
- **WHEN** authenticated user sends `GET /api/dashboard`
- **THEN** the system SHALL return a JSON array of chart objects, each containing config fields and a `data` object with `points` and `summary`

#### Scenario: Create chart tile
- **WHEN** user sends `POST /api/dashboard` with valid chart configuration
- **THEN** the system SHALL create the chart tile and return the created object with computed data

#### Scenario: Unauthorized dashboard access
- **WHEN** an unauthenticated user sends `GET /api/dashboard`
- **THEN** the system SHALL return 401 Unauthorized

### Requirement: Chart data computation endpoint
The system SHALL provide a `POST /api/stats/chart-data` endpoint that accepts a chart configuration (metric, exerciseId, period) and returns computed data points. This endpoint SHALL be used for both preview and dashboard rendering.

#### Scenario: Compute chart data
- **WHEN** user sends `POST /api/stats/chart-data` with `{metric: "weight", exerciseId: 3, period: "30d"}`
- **THEN** the system SHALL return `{points: [{date, value}...], summary: {current, best, change, trend}}`

### Requirement: Gruvbox chart theming
Chart colors SHALL use the Gruvbox palette. Line charts SHALL use `var(--green)` for the line and points. Bar charts SHALL use `var(--green)` with 80% opacity for bars. Grid lines SHALL use `var(--border)` color. Text labels SHALL use `var(--fg-dim)`.

#### Scenario: Dark theme chart colors
- **WHEN** theme is dark
- **THEN** chart line/bars SHALL be #b8bb26, grid lines SHALL be #504945, text SHALL be #a89984

#### Scenario: Light theme chart colors
- **WHEN** theme is light
- **THEN** chart line/bars SHALL be #98971a, grid lines SHALL be #bdae93, text SHALL be #7c6f64
