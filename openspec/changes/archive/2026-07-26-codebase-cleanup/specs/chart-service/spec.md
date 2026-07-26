# Chart Service

## Purpose

Shared service for chart data computation, extracted from duplicated logic in `StatsController` and `DashboardController`.

## Requirements

### ComputeChartData

- Accept: userId, metric, exerciseId (nullable), period, startDate (optional)
- Support metrics: weight, volume, est1rm, reps, bodyWeight, frequency
- Support rolling periods: 7d, 30d, 90d, 180d, 365d, all
- Return: `List<ChartDataPoint>` with Date and Value properties
- Apply `RequiresExercise()` rules: bodyWeight, frequency, volume do not require exerciseId

### ComputeChartSummary

- Accept: data points list, metric
- Return: `ChartSummary` with CurrentValue, PreviousValue, Change, ChangePercent, Unit
- Calculate change vs previous period

### GetCutoffDate

- Accept: period string
- Return: DateTime cutoff for the period
- Handle 'all' as no cutoff (DateTime.MinValue)

## API

```
GET /api/stats/chart-data          → StatsController (existing, delegates to ChartService)
POST /api/stats/chart-data         → DashboardController (existing, delegates to ChartService)
```

No new endpoints. ChartService is internal only.

## Implementation

- File: `backend/Services/ChartService.cs`
- Inject `GymDbContext` via constructor
- Use typed models: `ChartDataPoint`, `ChartSummary` (move from StatsController or create in Models/)
- Both StatsController and DashboardController inject and call ChartService
