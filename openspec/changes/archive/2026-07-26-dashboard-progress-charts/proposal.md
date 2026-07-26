## Why

The current stats page shows fixed, non-interactive CSS bar charts with no filtering. Users cannot choose what data to visualize or how. A configurable dashboard lets users track the specific progress metrics that matter to them — weight progression on key lifts, volume trends, body weight changes — with charts that persist across sessions.

## What Changes

- Replace the fixed stats page with a user-configurable chart dashboard
- Users add chart tiles by selecting: metric, exercise (when applicable), period, chart type
- Chart configurations persist per user in the database (new `DashboardCharts` table)
- Rolling time periods (last 7 days, 30 days, 90 days, etc.) that always show current data
- Live chart preview when adding or editing a tile
- Charts rendered with Chart.js (line and bar types) with Gruvbox theming
- Users can edit, delete, and reorder their dashboard tiles
- Summary stats per chart (current value, best, trend direction)

## Capabilities

### New Capabilities

- `dashboard-charts`: User-configurable progress dashboard with persistent chart tiles, chart data computation, and live preview

### Modified Capabilities

- `workout-logs`: Workout data model unchanged, but dashboard reads from existing workout/exercise tables to compute chart data

## Impact

- **Backend**: New `DashboardChart` model, EF Core migration, `DashboardController` (CRUD + reorder), `POST /api/stats/chart-data` endpoint for chart data computation
- **Frontend**: New `ProgressPage` component replacing `StatsPage`, `ChartTile` component, `ChartEditor` modal with preview, `DashboardService`, Chart.js + ng2-charts dependency
- **Database**: New `DashboardCharts` table with foreign key to `Users`
- **Routing**: `/progress` route replaces or supplements `/stats`
- **Dependencies**: `chart.js` + `ng2-charts` (Angular Chart.js wrapper, ~60KB gzipped)
