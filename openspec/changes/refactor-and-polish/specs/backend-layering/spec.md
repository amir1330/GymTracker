## ADDED Requirements

### Requirement: Controllers SHALL be thin HTTP handlers
Controllers SHALL only handle HTTP request/response mapping. Business logic and database access SHALL be delegated to service classes. Controllers SHALL NOT inject `GymDbContext` directly (except AuthController for user creation via UserManager).

#### Scenario: Controller delegates to service
- **WHEN** a controller receives a request
- **THEN** it calls a service method and returns the result as an HTTP response

#### Scenario: No direct DbContext in controllers
- **WHEN** a controller needs to query or modify data
- **THEN** it calls a service method instead of using `_context` directly

### Requirement: Services SHALL encapsulate business logic
Each domain area (Workouts, Exercises, Presets, Stats, Dashboard, User) SHALL have a corresponding service class registered in the DI container. Services SHALL own all database operations and business rule validation.

#### Scenario: Service handles CRUD operations
- **WHEN** a service receives a create/update/delete request
- **THEN** it performs the database operation and returns the result

#### Scenario: Service registered in DI
- **WHEN** the application starts
- **THEN** all services are registered in the DI container with Scoped lifetime

### Requirement: DTOs SHALL be in a dedicated DTOs/ folder
All request and response DTOs SHALL be extracted from controller files into a `DTOs/` folder organized by domain. Each DTO SHALL be in its own file.

#### Scenario: DTOs organized by domain
- **WHEN** a developer looks for workout-related DTOs
- **THEN** they find them in `DTOs/Workouts/` directory

#### Scenario: No inline DTOs in controllers
- **WHEN** a controller file is opened
- **THEN** it contains no DTO class definitions

### Requirement: AutoMapper SHALL be used for entity mapping
AutoMapper profiles SHALL be created for each domain area. Entity-to-DTO and DTO-to-entity mappings SHALL use AutoMapper instead of manual object initializers.

#### Scenario: Profile defines mappings
- **WHEN** the application starts
- **THEN** AutoMapper profiles are registered and entity↔DTO mappings are available

#### Scenario: Controller uses mapped responses
- **WHEN** a controller returns data from a service
- **THEN** the response is a mapped DTO, not an anonymous object

### Requirement: N+1 query issues SHALL be fixed
DashboardController.GetAll() SHALL load chart data in batch, not per-chart in a loop. DashboardController.Reorder() SHALL load all charts to reorder in a single query.

#### Scenario: GetAll loads chart data efficiently
- **WHEN** a user has N dashboard charts
- **THEN** the endpoint executes at most 2-3 database queries regardless of N

#### Scenario: Reorder loads charts in batch
- **WHEN** a user reorders M charts
- **THEN** the endpoint executes at most 2 database queries (load + save) regardless of M

### Requirement: StatsController SHALL return typed DTOs
StatsController SHALL return typed response DTOs instead of anonymous objects for all endpoints.

#### Scenario: Stats endpoint returns typed response
- **WHEN** a client calls the stats endpoint
- **THEN** the response body matches a defined DTO schema
