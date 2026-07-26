## Context

The gym tracker app currently has a fixed stats page (`/stats`) that shows summary cards (total workouts, weekly count, monthly count, total volume) and CSS bar charts for exercise frequency, muscle group distribution, workout calendar, and per-exercise progress. These charts are non-interactive, have no filtering, and use basic CSS bars instead of real chart libraries.

The user wants a configurable dashboard where they choose what metrics to visualize, which exercises to track, what time periods to use, and what chart types to display. Charts persist across sessions.

**Current state**: Stats page with hardcoded views, `StatsController` with summary + per-exercise endpoints, no user-specific chart configuration storage.

**Tech stack**: ASP.NET 8 backend, Angular 22 frontend, PostgreSQL, EF Core, JWT auth, zoneless Angular with `ChangeDetectorRef.markForCheck()` pattern.

## Goals / Non-Goals

**Goals:**
- User can add multiple independent chart tiles to a dashboard
- Each tile is fully configurable: metric, exercise (when applicable), period, chart type
- Chart configurations persist in the database per user
- Rolling time periods (7d, 30d, 90d, 180d, 365d, all time)
- Live preview when adding/editing a chart tile
- Charts rendered with Chart.js (real line and bar charts)
- Gruvbox-themed chart colors
- Summary stats per chart (current value, best, trend)
- Edit, delete, and reorder tiles

**Non-Goals:**
- Drag-and-drop reordering (use up/down buttons instead)
- Sharing dashboards between users
- Real-time / WebSocket chart updates
- Custom date ranges (fixed rolling periods only)
- Export charts as images
- Multiple chart types beyond line and bar

## Decisions

### 1. Chart library: Chart.js via ng2-charts

**Choice**: `chart.js` + `ng2-charts` (Angular wrapper)

**Alternatives considered**:
- **uPlot**: Smaller (~35KB) but limited chart types, no built-in tooltips, steeper API
- **ECharts**: Feature-rich but heavy (~300KB), overkill for line/bar
- **D3.js**: Maximum flexibility but requires building everything from scratch
- **CSS bars (current)**: No real line charts possible, no tooltips, no interactivity

**Rationale**: Chart.js covers line + bar charts with tooltips, responsive design, and smooth curves. ng2-charts provides Angular integration. ~60KB gzipped is acceptable for a personal app.

### 2. Data computation: Server-side via dedicated endpoint

**Choice**: New `POST /api/stats/chart-data` endpoint that accepts chart config and returns computed data points.

**Alternatives considered**:
- **Client-side computation**: Fetch all workouts, compute in browser. Simpler but slower for large datasets, no caching.
- **Reuse existing `/api/stats` endpoint**: Too rigid, returns fixed structure.

**Rationale**: Server-side computation keeps the frontend thin, allows SQL-level aggregation, and the same endpoint serves both preview (unsaved config) and dashboard rendering (saved configs). One `GET /api/dashboard` call returns all chart configs + their data in a single response.

### 3. Dashboard data fetching: Single aggregated response

**Choice**: `GET /api/dashboard` returns all chart tiles with their computed data in one response. No waterfall of per-chart API calls.

**Rationale**: Avoids N+1 API calls. The typical user will have 3-8 charts. Each chart's data computation is a SQL query. Batch them in one request.

### 4. Period handling: Rolling windows computed server-side

**Choice**: Period is an enum (`7d`, `30d`, `90d`, `180d`, `365d`, `all`). Server computes `DateTime.UtcNow - period` for each query.

**Rationale**: Simple, always shows current data. No stale date ranges. Custom date ranges add UI complexity for minimal value in a gym tracker.

### 5. Chart config storage: New `DashboardCharts` table

**Choice**: New EF Core model `DashboardChart` with FK to `User`. Stores label, metric, exerciseId, period, chartType, position.

**Alternatives considered**:
- **Store in UserSettings as JSON**: Loses queryability, harder to validate
- **Store in localStorage**: Not cross-device, lost on clear

**Rationale**: Proper relational storage allows server-side validation, cross-device sync, and future features like sharing.

### 6. Reordering: Position field with up/down buttons

**Choice**: `position` integer column on `DashboardChart`. Up/down buttons call `PUT /api/dashboard/reorder` with new positions.

**Alternatives considered**:
- **Drag-and-drop**: Requires additional library (e.g., `@angular/cdk/drag-drop`), more complex
- **Alphabetical/fixed order**: No user control

**Rationale**: Position field is simple, works on mobile, no additional dependencies.

### 7. Metric ↔ Exercise coupling

**Choice**: Exercise selector visibility depends on selected metric:
- `bodyWeight`, `frequency` → exercise selector hidden
- `weight`, `est1rm`, `reps` → exercise selector required (single exercise)
- `volume` → exercise selector optional with "All exercises" option

**Rationale**: Some metrics are inherently per-exercise (weight progression), others are aggregatable (total volume). The UI should enforce this rather than letting users create meaningless configurations.

## Risks / Trade-offs

- **[Risk] Chart.js bundle size (~60KB gzipped)** → Acceptable for a personal app. Could lazy-load the chart module to avoid impacting initial load.
- **[Risk] Single aggregated dashboard response could be slow with many charts** → Mitigate by limiting max charts per user (e.g., 20). Each chart's data is a fast SQL query.
- **[Risk] Chart preview makes API calls on every config change** → Debounce preview requests (300ms) to avoid spamming the server.
- **[Trade-off] No custom date ranges** → Rolling periods cover 95% of use cases. Custom ranges add date picker UI complexity.
- **[Trade-off] No drag-and-drop reorder** → Up/down buttons are simpler and work everywhere. Can upgrade later.

## Migration Plan

1. Create `DashboardChart` model and EF Core migration
2. Add `DashboardController` with CRUD + reorder endpoints
3. Add `POST /api/stats/chart-data` endpoint
4. Install `chart.js` + `ng2-charts` in Angular project
5. Create `DashboardService`, `ProgressPage`, `ChartTile`, `ChartEditor` components
6. Update routing: `/progress` route, update nav links
7. Theme chart colors to Gruvbox palette
8. Test with existing workout data

**Rollback**: Delete the `DashboardCharts` table and revert frontend components. No changes to existing data models.

## Open Questions

- Should there be a max limit on charts per dashboard? (Propose: 20)
- Should the old `/stats` page be removed or kept alongside the new dashboard?
