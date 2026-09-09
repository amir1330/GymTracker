## ADDED Requirements

### Requirement: No N+1 queries in service methods
All service methods SHALL execute database queries outside of loops. Batch loading MUST be used instead of per-iteration queries.

#### Scenario: Dashboard chart data loading
- **WHEN** `DashboardService.GetAllChartDataAsync` loads data for multiple charts
- **THEN** all workout data SHALL be fetched in a single batch query, not per-chart

### Requirement: Every controller has a dedicated service
Business logic SHALL reside in service classes, not in controllers. Controllers SHALL only handle HTTP concerns (parameter binding, response formatting).

#### Scenario: AuthController delegates to service
- **WHEN** a user registers or logs in
- **THEN** `AuthController` SHALL delegate user creation, settings creation, and token generation to `AuthService`

### Requirement: DTO layer independence
DTOs SHALL NOT depend on service-layer types. All shared types used by DTOs MUST be defined in the DTO layer.

#### Scenario: DashboardChartData references only DTO types
- **WHEN** `DashboardChartData` is defined
- **THEN** it SHALL reference `ChartDataPoint` and `ChartSummary` from the DTO namespace, not from `ChartService`

### Requirement: Dead code removed
All unused services, methods, properties, and endpoints SHALL be removed. No DI registration for unused services.

#### Scenario: StatsService removed
- **WHEN** the application starts
- **THEN** `StatsService` SHALL NOT be registered in the DI container and the file SHALL be deleted

#### Scenario: CreateFromPreset endpoint removed
- **WHEN** `POST /api/workouts/from-preset/{presetId}` is called
- **THEN** the endpoint SHALL return 404 (no longer exists)

#### Scenario: WorkoutExercise.RestTime removed
- **WHEN** a workout exercise is serialized
- **THEN** the `restTime` property SHALL NOT be present in the JSON response

### Requirement: Duplicate DTOs consolidated
Identical request DTOs SHALL be merged into a single shared type.

#### Scenario: Create/Update dashboard chart use same DTO
- **WHEN** creating or updating a dashboard chart
- **THEN** both endpoints SHALL accept the same `DashboardChartRequest` DTO

### Requirement: Consistent model defaults
The `UserSettings.Theme` default value SHALL be consistent between the EF model and the AuthController registration logic.

#### Scenario: New user gets auto theme
- **WHEN** a new user registers
- **THEN** their `UserSettings.Theme` SHALL be set to `"auto"` in both the model default and the controller code

### Requirement: Navigation properties serialized safely
All navigation properties that could cause circular references SHALL have `[JsonIgnore]`.

#### Scenario: UserSettings.User is not serialized
- **WHEN** `UserSettings` is serialized
- **THEN** the `User` navigation property SHALL NOT appear in the output

### Requirement: DbContext and Migrations colocated
The `GymDbContext` file and the `Migrations` folder SHALL be in the same parent directory.

#### Scenario: Data folder contains both
- **WHEN** viewing the backend directory structure
- **THEN** `GymDbContext.cs` and `Migrations/` SHALL both be in `Data/`

## REMOVED Requirements

### Requirement: Workout from preset endpoint
**Reason**: Frontend builds workouts client-side from preset data; the endpoint is never called.
**Migration**: Frontend maps preset exercises to workout exercises in `workout-form.ts` via `PresetService.getAll()`.
