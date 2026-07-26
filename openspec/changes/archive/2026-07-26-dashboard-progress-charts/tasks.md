## 1. Backend - Data Model

- [x] 1.1 Create `DashboardChart` model with fields: Id, UserId, Label, Metric, ExerciseId (nullable FK), Period, ChartType, Position
- [x] 1.2 Add `DbSet<DashboardChart>` to `GymDbContext`
- [x] 1.3 Add EF Core entity configuration for `DashboardChart` (PK, FK to User, FK to Exercise nullable)
- [x] 1.4 Create and apply EF Core migration for `DashboardCharts` table

## 2. Backend - Chart Data Computation

- [x] 2.1 Create `POST /api/stats/chart-data` endpoint accepting `{metric, exerciseId?, period}` and returning `{points: [{date, value}], summary: {current, best, change, trend}}`
- [x] 2.2 Implement weight metric computation (heaviest set per session for given exercise)
- [x] 2.3 Implement volume metric computation (sets × reps × weight, optionally filtered by exercise)
- [x] 2.4 Implement estimated 1RM metric computation (max of weight × (1 + reps/30) per session)
- [x] 2.5 Implement reps metric computation (total reps per session for given exercise)
- [x] 2.6 Implement bodyWeight metric computation (workout bodyWeight values)
- [x] 2.7 Implement frequency metric computation (workouts per week)
- [x] 2.8 Implement rolling period filtering (7d, 30d, 90d, 180d, 365d, all) using `DateTime.UtcNow`
- [x] 2.9 Implement summary computation (current, best, percentage change, trend direction)

## 3. Backend - Dashboard CRUD

- [x] 3.1 Create `DashboardController` with `GET /api/dashboard` (returns all user charts with computed data)
- [x] 3.2 Implement `POST /api/dashboard` (create chart tile, validate metric/exercise coupling)
- [x] 3.3 Implement `PUT /api/dashboard/{id}` (update chart config)
- [x] 3.4 Implement `DELETE /api/dashboard/{id}` (remove chart tile)
- [x] 3.5 Implement `PUT /api/dashboard/reorder` (accept `[{id, position}]`, update positions)
- [x] 3.6 Add validation: max 20 charts per user, metric-specific exercise requirements

## 4. Frontend - Dependencies & Service

- [x] 4.1 Install `chart.js` and `ng2-charts` packages
- [x] 4.2 Create `DashboardService` with methods: getAll, create, update, delete, reorder, getChartData
- [x] 4.3 Define TypeScript interfaces: `DashboardChart`, `ChartData`, `ChartPoint`, `ChartSummary`

## 5. Frontend - Chart Components

- [x] 5.1 Create `ChartTile` component that renders a single Chart.js chart (line or bar) with Gruvbox colors
- [x] 5.2 Implement Gruvbox chart theme config (green lines/bars, border grid, dim text)
- [x] 5.3 Create `ChartEditor` component (add/edit modal with form: label, metric radio, exercise select, period radio, chart type radio)
- [x] 5.4 Implement metric ↔ exercise coupling (hide exercise for bodyWeight/frequency, require for weight/est1rm/reps, optional "All" for volume)
- [x] 5.5 Implement live preview in `ChartEditor` (debounced API call, chart renders in modal)

## 6. Frontend - Dashboard Page

- [x] 6.1 Create `ProgressPage` component with dashboard grid layout
- [x] 6.2 Implement empty state ("No charts yet" with add prompt)
- [x] 6.3 Render `ChartTile` components from dashboard data with summary stats (current, change%, trend arrow)
- [x] 6.4 Add edit/delete/up/down controls on each tile
- [x] 6.5 Implement add chart flow (open `ChartEditor` modal, save, refresh dashboard)
- [x] 6.6 Implement edit chart flow (open `ChartEditor` pre-filled, save, refresh dashboard)
- [x] 6.7 Implement delete chart flow (confirm, delete, refresh dashboard)
- [x] 6.8 Implement reorder flow (up/down buttons, call reorder API, refresh dashboard)

## 7. Routing & Integration

- [x] 7.1 Add `/progress` route to app routing (replaces or supplements `/stats`)
- [x] 7.2 Update navigation links to point to `/progress` instead of `/stats`
- [x] 7.3 Lazy-load the progress page module for performance
- [x] 7.4 Remove or deprecate old `StatsPage` component and `/stats` route

## 8. Styling

- [x] 8.1 Style dashboard grid layout (responsive, 1-2 columns based on screen width)
- [x] 8.2 Style `ChartEditor` modal with Gruvbox theme
- [x] 8.3 Style `ChartTile` cards with borders, controls, summary stats
- [x] 8.4 Style chart summary bar (current value, change%, trend icon)
- [x] 8.5 Ensure mobile responsiveness of dashboard layout
