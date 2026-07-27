# GymTracker — Architecture Guide

A beginner-friendly walkthrough of every layer, pattern, and connection in this full-stack gym tracking app.

---

## Table of Contents

1. [Big Picture](#1-big-picture)
2. [Backend Architecture (ASP.NET 8)](#2-backend-architecture)
3. [Frontend Architecture (Angular 22)](#3-frontend-architecture)
4. [Database (PostgreSQL)](#4-database)
5. [Authentication](#5-authentication)
6. [API Endpoints](#6-api-endpoints)
7. [Key Patterns](#7-key-patterns)
8. [File Structure](#8-file-structure)

---

## 1. Big Picture

```
┌───────────────────────────────────────────────────────────────┐
│                      USER'S BROWSER                          │
│                                                               │
│  ┌─────────────────────────────────────────────────────────┐  │
│  │              Angular 22 SPA (port 4200)                 │  │
│  │                                                         │  │
│  │  pages/              components/       services/        │  │
│  │  ┌──────────┐        ┌──────────┐     ┌──────────────┐ │  │
│  │  │ Login    │        │ ChartTile│     │WorkoutService│ │  │
│  │  │ Register │        │ ChartEd. │     │ExerciseServ. │ │  │
│  │  │ Workouts │◄──────►│ Onboard. │◄───►│PresetService │ │  │
│  │  │ Presets  │        └──────────┘     │DashboardServ.│ │  │
│  │  │ Exercises│                         │AuthService   │ │  │
│  │  │ Progress │    models/              │SettingsServ. │ │  │
│  │  │ Settings │    ┌──────────┐         └──────┬───────┘ │  │
│  │  └──────────┘    │Interfaces│                │         │  │
│  │                  └──────────┘     HTTP with JWT Bearer │  │
│  └───────────────────────┼───────────────────────────────┘  │
│                          │                                   │
└──────────────────────────┼───────────────────────────────────┘
                           │
                           ▼
┌───────────────────────────────────────────────────────────────┐
│                ASP.NET Web API (port 5000)                   │
│                                                               │
│  ┌─────────────────────────────────────────────────────┐     │
│  │  CONTROLLER LAYER  (thin HTTP handlers)             │     │
│  │  AuthController │ WorkoutsController │ Exercises... │     │
│  │  DashboardController │ PresetsController │ ...      │     │
│  └──────────────────────┬──────────────────────────────┘     │
│                         │ calls                               │
│  ┌──────────────────────▼──────────────────────────────┐     │
│  │  SERVICE LAYER  (business logic + DB access)        │     │
│  │  ExercisesService │ WorkoutsService │ PresetsService │     │
│  │  StatsService │ DashboardService │ UserService      │     │
│  │  JwtService │ ChartService                          │     │
│  └──────────────────────┬──────────────────────────────┘     │
│                         │ uses                                │
│  ┌──────────────────────▼──────────────────────────────┐     │
│  │  DTOs/  (request/response shapes)                   │     │
│  │  Auth/ │ Workouts/ │ Exercises/ │ Presets/          │     │
│  │  Stats/ │ Dashboard/ │ User/                        │     │
│  └──────────────────────┬──────────────────────────────┘     │
│                         │ maps via                            │
│  ┌──────────────────────▼──────────────────────────────┐     │
│  │  AutoMapper  (entity ↔ DTO conversion)              │     │
│  │  ExerciseProfile │ WorkoutProfile │ PresetProfile    │     │
│  │  UserProfile                                         │     │
│  └──────────────────────┬──────────────────────────────┘     │
│                         │ queries                             │
│  ┌──────────────────────▼──────────────────────────────┐     │
│  │  EF Core  (ORM → SQL)                               │     │
│  │  GymDbContext (IdentityDbContext<User>)              │     │
│  └──────────────────────┬──────────────────────────────┘     │
│                         │                                     │
└─────────────────────────┼─────────────────────────────────────┘
                          │
                          ▼
┌───────────────────────────────────────────────────────────────┐
│                PostgreSQL (port 5432)                         │
│                Database: gym_tracker                          │
│                                                               │
│  Tables: Users, Exercises, Workouts, WorkoutExercises,       │
│          Presets, PresetExercises, DashboardCharts,           │
│          UserSettings + ASP.NET Identity tables               │
└───────────────────────────────────────────────────────────────┘
```

### Request Lifecycle

1. User clicks a button in the Angular app
2. A component calls a service method (e.g., `workoutService.create(...)`)
3. The service makes an HTTP request to `http://127.0.0.1:5000/api/workouts`
4. Angular's HTTP interceptor **automatically attaches the JWT token** to the header
5. The request hits the ASP.NET backend
6. The `[Authorize]` attribute checks the JWT — if invalid/missing, returns 401
7. The **Controller** extracts the user ID from the token, validates input, delegates to a **Service**
8. The **Service** runs business logic, calls **EF Core** to query/modify PostgreSQL
9. The response flows back: PostgreSQL → EF Core → Service → Controller → HTTP → Angular service → Component → Template updates

---

## 2. Backend Architecture (ASP.NET 8)

### The Three-Layer Pattern

The backend uses **Controller → Service → EF Core DbContext** (no repository layer):

```
Controller          Service             DbContext
─────────          ───────             ─────────
HTTP routing       Business logic      Database queries
Input validation   Ownership checks    SQL generation
User ID extraction Data transformation Connection management
Return responses   Call DB via context  Change tracking
```

**Why no repository layer?** EF Core's `DbContext` already implements the Unit of Work and Repository patterns. Adding another layer on top would be redundant indirection for a project this size.

### Layer 1: Controllers (thin HTTP handlers)

Controllers handle **only** HTTP concerns. They:
- Extract the user ID from the JWT token
- Validate input (via DTOs and `[Required]` attributes)
- Call the appropriate service method
- Return the right HTTP status code

```csharp
[HttpPost]
[Authorize]
public async Task<IActionResult> Create([FromBody] ExerciseRequest request)
{
    // Get user from JWT
    var userId = int.Parse(_userManager.GetUserId(User)!);
    
    // Delegate to service
    var created = await _exercisesService.CreateAsync(exercise, userId);
    
    // Return HTTP response
    return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
}
```

Controllers **never**:
- Write SQL queries
- Call `_context` directly
- Contain business logic
- Define DTOs (they're in `DTOs/`)

### Layer 2: Services (business logic + DB access)

Services own all business logic and data access. Each service handles one domain:

| Service | Responsibility |
|---------|---------------|
| `ExercisesService` | CRUD, user-scoping, name uniqueness |
| `WorkoutsService` | CRUD, profile weight sync, preset loading |
| `PresetsService` | CRUD, user-scoping |
| `StatsService` | Aggregated statistics queries |
| `DashboardService` | Chart config CRUD, batch data loading |
| `UserService` | Profile and settings updates |
| `JwtService` | JWT token generation |
| `ChartService` | Chart data computation (weight, volume, 1RM, duration, etc.) |

**Example — ExercisesService:**

```csharp
public class ExercisesService
{
    private readonly GymDbContext _context;

    public ExercisesService(GymDbContext context)
    {
        _context = context;
    }

    // Returns default exercises + user's own exercises
    public async Task<List<Exercise>> GetAllAsync(int userId)
    {
        return await _context.Exercises
            .Where(e => e.IsDefault || e.UserId == userId)  // ← ownership filter
            .OrderBy(e => e.MuscleGroup)
            .ThenBy(e => e.Name)
            .ToListAsync();
    }

    // Only allows creating if name isn't taken by defaults or this user
    public async Task<Exercise> CreateAsync(Exercise exercise, int userId)
    {
        exercise.UserId = userId;
        exercise.IsDefault = false;
        _context.Exercises.Add(exercise);
        await _context.SaveChangesAsync();
        return exercise;
    }
}
```

**Key pattern — user scoping:** Every query filters by `UserId` (or includes `IsDefault` records). This ensures users never see or modify each other's data.

### Layer 3: DTOs (Data Transfer Objects)

DTOs define the shape of API request/response data. They're separate from database models:

```
DTOs/
├── Auth/
│   ├── RegisterRequest.cs      { username, email, password, confirmPassword }
│   └── LoginRequest.cs         { email, password }
├── Workouts/
│   ├── CreateWorkoutRequest.cs { date, notes, bodyWeight, workoutExercises }
│   ├── UpdateWorkoutRequest.cs
│   ├── WorkoutExerciseRequest.cs { exerciseId, sets, reps, weight, duration }
│   └── WorkoutResponse.cs
├── Exercises/
│   └── ExerciseRequest.cs      { name, muscleGroup, isDuration, durationUnit }
├── Presets/
│   ├── CreatePresetRequest.cs
│   ├── PresetExerciseRequest.cs
│   └── PresetResponse.cs
├── Stats/
│   └── StatsResponse.cs
├── Dashboard/
│   ├── CreateDashboardChartRequest.cs
│   └── ReorderRequest.cs
└── User/
    ├── UpdateProfileRequest.cs
    └── UpdateSettingsRequest.cs
```

**Why DTOs instead of using models directly?**
- Models have navigation properties (e.g., `Workout.WorkoutExercises`) that cause infinite recursion in JSON serialization
- DTOs expose only what the client needs (no internal IDs, no server-managed fields)
- Request DTOs can omit fields the server sets automatically (like `UserId`)

### AutoMapper (entity ↔ DTO conversion)

AutoMapper eliminates manual mapping code. Each domain has a Profile class:

```csharp
// Mappings/ExerciseProfile.cs
public class ExerciseProfile : Profile
{
    public ExerciseProfile()
    {
        CreateMap<ExerciseRequest, Exercise>();
        CreateMap<Exercise, ExerciseResponse>();
    }
}
```

In controllers, mapping is one line:
```csharp
var exercise = _mapper.Map<Exercise>(request);  // DTO → Entity
```

### DI (Dependency Injection)

ASP.NET creates and manages service instances. In `Program.cs`:
```csharp
builder.Services.AddScoped<ExercisesService>();   // One instance per HTTP request
builder.Services.AddScoped<WorkoutsService>();
builder.Services.AddAutoMapper(typeof(Program));  // Scan for Profile classes
```

Controllers receive dependencies via constructor injection:
```csharp
public ExercisesController(ExercisesService exercisesService, UserManager<User> userManager, IMapper mapper)
```

### N+1 Query Fix (DashboardService)

**Before (N+1 problem):**
```csharp
foreach (var chart in charts) {
    var data = await GetChartDataAsync(chart);  // ← 1 query per chart = N+1!
}
```

**After (batch loading):**
```csharp
// Load all workout data in one query, compute per chart in memory
var allWorkouts = await _context.WorkoutExercises
    .Where(we => chartIds.Contains(we.WorkoutId))
    .ToListAsync();  // ← 1 query total
```

---

## 3. Frontend Architecture (Angular 22)

### Folder Structure

```
src/app/
├── pages/                  Route-level components (each = one URL)
│   ├── login/              /login
│   ├── register/           /register
│   ├── workout-list/       /workouts
│   ├── workout-form/       /workouts/new, /workouts/:id/edit
│   ├── preset-list/        /presets
│   ├── preset-form/        /presets/new, /presets/:id/edit
│   ├── exercise-list/      /exercises
│   ├── exercise-form/      /exercises/new, /exercises/:id/edit
│   ├── progress/           /progress (charts dashboard)
│   └── settings/           /settings (profile, theme)
├── components/             Shared child components (used by pages)
│   ├── chart-tile/         Single chart card with Chart.js
│   ├── chart-editor/       Chart config modal
│   └── onboarding-guide/   First-time user guide overlay
├── models/                 TypeScript interfaces (one per file)
│   ├── exercise.model.ts   Exercise, DurationUnit
│   ├── workout.model.ts    Workout, WorkoutExercise
│   ├── preset.model.ts     Preset, PresetExercise
│   ├── dashboard.model.ts  DashboardChart, ChartData, ChartSummary
│   ├── user.model.ts       UserProfile
│   └── auth.model.ts       User
├── services/               HTTP services (one per domain)
│   ├── exercise.service.ts
│   ├── workout.service.ts
│   ├── preset.service.ts
│   ├── dashboard.service.ts
│   ├── settings.service.ts
│   └── auth.service.ts
├── guards/                 Route protection
│   └── auth.guard.ts       Redirects to /login if not authenticated
├── interceptors/           HTTP request modification
│   └── auth.interceptor.ts Attaches JWT token to all requests
├── app.config.ts           Providers, Chart.js setup, interceptor registration
├── app.routes.ts           URL → component mapping (lazy-loaded)
└── app.ts / app.html       Root component (nav bar + router-outlet)
```

### Components

Each component has three files:
- **`.ts`** — TypeScript class with logic, state, and service calls
- **`.html`** — Template with Angular syntax (`@if`, `@for`, `[(ngModel)]`)
- **`.css`** — Component-scoped styles

```typescript
@Component({
  selector: 'app-exercise-list',
  standalone: true,                          // No NgModule needed (Angular 22 default)
  imports: [CommonModule, RouterModule],
  templateUrl: './exercise-list.html'
})
export class ExerciseList implements OnInit {
  exercises: Exercise[] = [];               // Component state
  
  constructor(
    private exerciseService: ExerciseService,  // Injected service
    private cdr: ChangeDetectorRef            // For zoneless change detection
  ) {}
  
  ngOnInit() {
    this.exerciseService.getAll().subscribe(exercises => {
      this.exercises = exercises;
      this.cdr.markForCheck();  // ← Tell Angular to re-render
    });
  }
}
```

### Services (HTTP layer)

Services wrap Angular's `HttpClient` and return `Observable`s:

```typescript
@Injectable({ providedIn: 'root' })  // Singleton — one instance everywhere
export class ExerciseService {
  private apiUrl = '/api/exercises';
  
  constructor(private http: HttpClient) {}
  
  getAll(): Observable<Exercise[]> {
    return this.http.get<Exercise[]>(this.apiUrl);
  }
  
  create(exercise: Partial<Exercise>): Observable<Exercise> {
    return this.http.post<Exercise>(this.apiUrl, exercise);
  }
}
```

### Models (TypeScript interfaces)

Interfaces define the shape of data flowing between services and components:

```typescript
// models/exercise.model.ts
export type DurationUnit = 'seconds' | 'minutes' | 'hours';

export interface Exercise {
  id: number;
  name: string;
  muscleGroup: string;
  isDuration: boolean;
  durationUnit: DurationUnit;
  isDefault: boolean;
}
```

Components import from models, not from services:
```typescript
import { Exercise } from '../../models/exercise.model';
```

### Routing

Routes map URLs to lazy-loaded components:

```typescript
const routes: Routes = [
  { path: '', redirectTo: '/progress', pathMatch: 'full' },
  { path: 'login', loadComponent: () => import('./pages/login/login').then(m => m.Login) },
  { path: 'workouts', loadComponent: () => import('./pages/workout-list/workout-list').then(m => m.WorkoutList), canActivate: [authGuard] },
  { path: 'exercises', loadComponent: () => import('./pages/exercise-list/exercise-list').then(m => m.ExerciseList), canActivate: [authGuard] },
  // ... more routes
];
```

**Key concepts:**
- `loadComponent: () => import(...)` — **lazy loading**: JS for that page is only downloaded when the user navigates to it
- `canActivate: [authGuard]` — redirects to `/login` if not authenticated
- `:id` in path — URL parameter read by `ActivatedRoute`

### Change Detection (zoneless Angular 22)

Angular 22 defaults to **zoneless** change detection. Async HTTP callbacks don't automatically trigger re-renders. Every component must call `markForCheck()`:

```typescript
this.exerciseService.getAll().subscribe({
  next: (exercises) => {
    this.exercises = exercises;
    this.cdr.markForCheck();  // ← Without this, the template stays stale
  }
});
```

### Auth Interceptor

Every HTTP request passes through the interceptor, which attaches the JWT token:

```typescript
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  if (req.url.includes('/api/auth/')) return next(req);  // Skip for login/register
  
  const token = inject(AuthService).getToken();
  if (token) {
    req = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
  }
  
  return next(req).pipe(
    catchError(err => {
      if (err.status === 401) {
        inject(AuthService).logout();  // Token expired
        inject(Router).navigate(['/login']);
      }
      return throwError(() => err);
    })
  );
};
```

### Charts (Chart.js via ng2-charts)

The progress page uses **Chart.js** through **ng2-charts**:

```html
<canvas baseChart
        [type]="'line'"
        [data]="chartData"
        [options]="chartOptions">
</canvas>
```

Data flow:
1. Backend computes data points (date/value pairs) from workout history
2. Frontend maps them into Chart.js format: `{ labels: [...], datasets: [{ data: [...] }] }`
3. Chart.js renders a line/bar chart on the canvas

### Theming (CSS Variables)

The entire color scheme uses CSS custom properties:

```css
:root {                              /* Dark theme (default) */
  --bg: #282828;
  --fg: #ebdbb2;
  --green: #b8bb26;
}

[data-theme="light"] {              /* Light theme */
  --bg: #fbf1c7;
  --fg: #3c3836;
  --green: #98971a;
}
```

Switching themes is just toggling an attribute on `<html>`. Every element using `var(--bg)` automatically updates.

---

## 4. Database (PostgreSQL)

### Entity Relationship Diagram

```
┌──────────┐     ┌──────────────┐     ┌──────────────────┐
│   User   │────▶│ UserSettings │     │    Exercise      │
│          │     │ (1:1)        │     │                  │
│ Id       │     │ Theme        │     │ Id               │
│ UserName │     │ RestTimer    │     │ Name             │
│ Email    │     └──────────────┘     │ MuscleGroup      │
│ Weight   │                          │ IsDuration       │
│ Height   │                          │ DurationUnit     │
└────┬─────┘                          │ IsDefault        │
     │                                │ UserId? ◀────────┼── nullable (null = default exercise)
     │ 1:*                            └────────┬─────────┘
     │                                          │ *:1
     │                                          │
┌────┴──────┐     ┌──────────────────┐          │
│  Workout  │────▶│ WorkoutExercise  │──────────┘
│           │     │                  │
│ Date      │     │ Sets, Reps       │
│ Notes     │     │ Weight, Duration │
│ BodyWeight│     │ DurationUnit     │
│ UserId    │     │ RestTime         │
└───────────┘     └──────────────────┘

┌──────────┐     ┌──────────────────┐
│  Preset  │────▶│ PresetExercise   │────▶ Exercise
│          │     │                  │
│ Name     │     │ DefaultSets      │
│ UserId   │     │ DefaultReps      │
└──────────┘     │ DefaultWeight    │
                 │ DefaultDuration  │
                 └──────────────────┘

┌──────────────────┐
│ DashboardChart   │────▶ Exercise (optional)
│                  │
│ Label, Metric    │
│ Period, ChartType│
│ Position, UserId │
└──────────────────┘
```

### Entity Framework Core (ORM)

EF Core maps C# classes to database tables:

```csharp
// This C# class...
public class Workout {
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? Notes { get; set; }
    public ICollection<WorkoutExercise> WorkoutExercises { get; set; }
}

// ...maps to this SQL table:
// CREATE TABLE "Workouts" (
//     "Id" SERIAL PRIMARY KEY,
//     "UserId" INTEGER REFERENCES "Users"("Id"),
//     "Notes" VARCHAR(500)
// );
```

When you write `_context.Workouts.Add(workout)`, EF Core generates the INSERT SQL automatically.

### GymDbContext

The DbContext defines which tables exist and how entities relate:

```csharp
public class GymDbContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<Workout> Workouts => Set<Workout>();
    public DbSet<Preset> Presets => Set<Preset>();
    public DbSet<DashboardChart> DashboardCharts => Set<DashboardChart>();
    // ...
}
```

### Migrations

Migrations track database schema changes:

```bash
dotnet ef migrations add AddExerciseUserId    # Generate migration code
dotnet ef database update                     # Apply to database
```

On app startup, `context.Database.Migrate()` applies any pending migrations automatically.

### Seed Data

31 default exercises are seeded on first run (`Data/SeedData.cs`). These have `IsDefault = true` and no `UserId`, making them visible to all users.

---

## 5. Authentication

### The Complete Auth Flow

```
REGISTER:
1. User fills form: username, email, password, confirmPassword
2. POST /api/auth/register
3. Backend validates: username unique, passwords match, min length
4. Backend: create User record (password hashed with PBKDF2)
5. Backend: create UserSettings with defaults (theme=dark, restTimer=90s)
6. Backend: generate JWT token (userId, username, email; expires in 60 min)
7. Return token to frontend
8. Frontend: store token in localStorage
9. Frontend: redirect to /progress

LOGIN:
1. User enters email + password
2. POST /api/auth/login
3. Backend: find user by email, verify password hash
4. Backend: generate JWT token
5. Return token
6. Frontend: store token, redirect to /progress

SUBSEQUENT REQUESTS:
1. Frontend interceptor reads token from localStorage
2. Adds header: Authorization: Bearer <token>
3. Backend validates token signature and expiration
4. If valid, extracts userId from token claims
5. Controller processes request for that user
```

### JWT Token Structure

```
Header:    { "alg": "HS256" }
Payload:   { "nameid": "42",          ← user ID
              "name": "john",          ← username
              "email": "john@example.com",
              "exp": 1722000000,       ← expiration (60 min)
              "iss": "GymTracker" }    ← issuer
Signature: HMAC-SHA256(header + payload, secret_key)
```

The token is **signed**, not encrypted. Anyone can read the payload, but nobody can forge it without the secret key.

### Password Hashing

Passwords are never stored in plain text. ASP.NET Identity uses PBKDF2:
```
Plain password: "Admin123!"
     ↓ PBKDF2 (600,000 iterations + salt)
Hashed password: "AQAAAAEAACcQAAAAENx..."
```

---

## 6. API Endpoints

### Auth (no login required)

| Method | Endpoint | Body | Response |
|--------|----------|------|----------|
| POST | `/api/auth/register` | `{ username, email, password, confirmPassword, weight?, height? }` | `{ token, userId }` |
| POST | `/api/auth/login` | `{ email, password }` | `{ token, userId }` |

### Exercises (user-scoped)

| Method | Endpoint | Body | Response |
|--------|----------|------|----------|
| GET | `/api/exercises` | — | Default + user's exercises |
| GET | `/api/exercises/:id` | — | Single exercise (if owned or default) |
| POST | `/api/exercises` | `{ name, muscleGroup, isDuration, durationUnit }` | Created exercise |
| PUT | `/api/exercises/:id` | `{ name, muscleGroup, isDuration, durationUnit }` | Updated (only if owned) |
| DELETE | `/api/exercises/:id` | — | 204 (only if owned, not default, not in use) |

### Workouts (user-scoped)

| Method | Endpoint | Body | Response |
|--------|----------|------|----------|
| GET | `/api/workouts` | — | User's workouts |
| GET | `/api/workouts/:id` | — | Single workout |
| POST | `/api/workouts` | `{ date, notes, bodyWeight, workoutExercises }` | Created workout |
| POST | `/api/workouts/from-preset/:presetId` | — | Workout created from preset |
| PUT | `/api/workouts/:id` | `{ date, notes, bodyWeight, workoutExercises }` | Updated workout |
| DELETE | `/api/workouts/:id` | — | 204 |

### Presets (user-scoped)

| Method | Endpoint | Body | Response |
|--------|----------|------|----------|
| GET | `/api/presets` | — | User's presets |
| POST | `/api/presets` | `{ name, presetExercises }` | Created preset |
| PUT | `/api/presets/:id` | `{ name, presetExercises }` | Updated preset |
| DELETE | `/api/presets/:id` | — | 204 |

### Dashboard (user-scoped)

| Method | Endpoint | Body | Response |
|--------|----------|------|----------|
| GET | `/api/dashboard` | — | Charts with computed data |
| POST | `/api/dashboard` | `{ label, metric, exerciseId?, period, chartType }` | Created chart |
| PUT | `/api/dashboard/:id` | `{ label, metric, exerciseId?, period, chartType }` | Updated chart |
| DELETE | `/api/dashboard/:id` | — | 204 |
| PUT | `/api/dashboard/reorder` | `[{ id, position }]` | 200 |

### Stats

| Method | Endpoint | Body | Response |
|--------|----------|------|----------|
| GET | `/api/stats` | — | Aggregated stats |
| POST | `/api/stats/chart-data` | `{ metric, exerciseId?, period }` | `{ points, summary }` |

### User

| Method | Endpoint | Body | Response |
|--------|----------|------|----------|
| GET | `/api/user/profile` | — | User profile |
| PUT | `/api/user/profile` | `{ weight?, height? }` | Updated profile |
| PUT | `/api/user/settings` | `{ theme, restTimer }` | Updated settings |

---

## 7. Key Patterns

### User Ownership

Every user-scoped entity (Workout, Preset, Exercise, DashboardChart) has a `UserId` field. Services always filter by the current user:

```csharp
// Service filters queries by user
.Where(e => e.IsDefault || e.UserId == userId)

// Service rejects unauthorized updates
if (existing.UserId != userId) return null;

// Controller returns 403 for cross-user access
if (!await _exercisesService.IsOwnedByAsync(id, UserId))
    return Forbid();
```

Default exercises (`IsDefault = true`) have `UserId = null` and are visible to all users.

### Delete-and-Reinsert for Nested Collections

When updating a workout's exercises, the backend deletes all existing `WorkoutExercise` rows and inserts the new ones:

```csharp
var existing = _context.WorkoutExercises.Where(we => we.WorkoutId == id);
_context.WorkoutExercises.RemoveRange(existing);

foreach (var e in request.Exercises) {
    workout.WorkoutExercises.Add(new WorkoutExercise { ... });
}
await _context.SaveChangesAsync();
```

**Why?** Simpler than diffing which exercises were added/removed/modified. Performance is fine for ~10 exercises per workout.

### Profile Weight Auto-Sync

When a workout is created with a body weight, the most recent workout's weight is synced to the user's profile:

```csharp
var latestWeight = await _context.Workouts
    .Where(w => w.UserId == userId && w.BodyWeight.HasValue)
    .OrderByDescending(w => w.Date)  // By user-set date, not insertion order
    .Select(w => w.BodyWeight)
    .FirstOrDefaultAsync();
```

### Chart Computation On-the-Fly

Charts are computed fresh every time the dashboard loads:
1. Query all relevant `WorkoutExercise` records
2. Group by day
3. Compute the metric (weight, volume, est1RM, duration, etc.)
4. Return data points + summary

No caching, no stale data. The dataset is small enough (one user's workouts) that computation is instant.

### Scoped DI Services

All services use `AddScoped<>` — one instance per HTTP request:
```csharp
builder.Services.AddScoped<ExercisesService>();  // Created per request, disposed after
```

This means services can safely hold references to `DbContext` (which is also scoped).

---

## 8. File Structure

```
backend/
├── Program.cs                     Entry point, DI registration, middleware
├── appsettings.json               Config (DB connection, JWT secret)
├── GymTracker.csproj              NuGet packages
├── Controllers/                   Thin HTTP handlers
│   ├── AuthController.cs          Login, register
│   ├── UserController.cs          Profile, settings
│   ├── ExercisesController.cs     Exercise CRUD
│   ├── WorkoutsController.cs      Workout CRUD
│   ├── PresetsController.cs       Preset CRUD
│   ├── StatsController.cs         Aggregated stats
│   └── DashboardController.cs     Chart config CRUD
├── Services/                      Business logic + DB access
│   ├── ExercisesService.cs
│   ├── WorkoutsService.cs
│   ├── PresetsService.cs
│   ├── StatsService.cs
│   ├── DashboardService.cs
│   ├── UserService.cs
│   ├── JwtService.cs              JWT token generation
│   └── ChartService.cs            Chart data computation
├── DTOs/                          Request/response shapes
│   ├── Auth/                      RegisterRequest, LoginRequest
│   ├── Workouts/                  CreateWorkoutRequest, WorkoutExerciseRequest, ...
│   ├── Exercises/                 ExerciseRequest
│   ├── Presets/                   CreatePresetRequest, PresetExerciseRequest, ...
│   ├── Stats/                     StatsResponse
│   ├── Dashboard/                 CreateDashboardChartRequest, ReorderRequest, ...
│   └── User/                      UpdateProfileRequest, UpdateSettingsRequest
├── Mappings/                      AutoMapper profiles
│   ├── ExerciseProfile.cs
│   ├── WorkoutProfile.cs
│   ├── PresetProfile.cs
│   └── UserProfile.cs
├── Models/                        Database entities
│   ├── User.cs                    Extends IdentityUser (Weight, Height, collections)
│   ├── UserSettings.cs            Theme, restTimer
│   ├── Exercise.cs                Name, MuscleGroup, IsDuration, DurationUnit, IsDefault, UserId?
│   ├── DurationUnit.cs            Enum: Seconds, Minutes, Hours
│   ├── Workout.cs                 Date, Notes, BodyWeight, UserId
│   ├── WorkoutExercise.cs         Sets, Reps, Weight, Duration, DurationUnit, RestTime
│   ├── Preset.cs                  Name, UserId
│   ├── PresetExercise.cs          DefaultSets, DefaultReps, DefaultWeight, DefaultDuration
│   └── DashboardChart.cs          Label, Metric, Period, ChartType, Position, UserId
├── Data/
│   ├── GymDbContext.cs            EF Core context (IdentityDbContext<User>)
│   └── SeedData.cs                31 default exercises
└── Migrations/                    Database schema history

frontend/
├── src/
│   ├── main.ts                    Bootstrap
│   ├── index.html                 HTML shell
│   ├── styles.css                 Global Gruvbox theme + base styles
│   ├── proxy.conf.json            Dev proxy: /api/* → localhost:5000
│   └── app/
│       ├── app.config.ts          Providers, Chart.js setup, interceptor
│       ├── app.routes.ts          URL → component mapping (lazy-loaded)
│       ├── app.ts / .html / .css   Root component (nav + router-outlet)
│       ├── pages/                  Route-level components
│       │   ├── login/
│       │   ├── register/
│       │   ├── workout-list/
│       │   ├── workout-form/
│       │   ├── preset-list/
│       │   ├── preset-form/
│       │   ├── exercise-list/
│       │   ├── exercise-form/
│       │   ├── progress/
│       │   └── settings/
│       ├── components/             Shared child components
│       │   ├── chart-tile/
│       │   ├── chart-editor/
│       │   └── onboarding-guide/
│       ├── models/                 TypeScript interfaces
│       │   ├── exercise.model.ts
│       │   ├── workout.model.ts
│       │   ├── preset.model.ts
│       │   ├── dashboard.model.ts
│       │   ├── user.model.ts
│       │   └── auth.model.ts
│       ├── services/               HTTP services
│       │   ├── exercise.service.ts
│       │   ├── workout.service.ts
│       │   ├── preset.service.ts
│       │   ├── dashboard.service.ts
│       │   ├── settings.service.ts
│       │   └── auth.service.ts
│       ├── guards/
│       │   └── auth.guard.ts
│       └── interceptors/
│           └── auth.interceptor.ts
```

---

## Quick Reference

### To add a new API endpoint:
1. Add a model class in `Models/`
2. Add a DbSet in `GymDbContext.cs`
3. Create a migration: `dotnet ef migrations add <Name>`
4. Add a DTO in `DTOs/<Domain>/`
5. Add an AutoMapper profile in `Mappings/`
6. Add a service method in `Services/`
7. Add a controller method in `Controllers/`

### To add a new Angular page:
1. Create component files (`.ts`, `.html`, `.css`) in `pages/<name>/`
2. Add a route in `app.routes.ts`
3. Create a service method for API calls (or a new service in `services/`)
4. Add a nav link in `app.html` (if needed)

### To change the color scheme:
Edit CSS variables in `frontend/src/styles.css`:
```css
:root {
  --green: #b8bb26;  /* Primary color */
  --red: #fb4934;    /* Danger color */
  --bg: #282828;     /* Background */
}
```

### To change the JWT expiration:
Edit `backend/appsettings.json`:
```json
{ "Jwt": { "ExpirationInMinutes": 60 } }
```
