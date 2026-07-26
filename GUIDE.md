# GymTracker — Complete Project Guide

A beginner-friendly walkthrough of every decision, pattern, and connection in this full-stack gym tracking app.

---

## Table of Contents

1. [Big Picture](#1-big-picture)
2. [How the Pieces Connect](#2-how-the-pieces-connect)
3. [Backend (ASP.NET)](#3-backend-aspnet)
4. [Frontend (Angular)](#4-frontend-angular)
5. [Database](#5-database)
6. [Authentication](#6-authentication)
7. [API Endpoints](#7-api-endpoints)
8. [Key Patterns Explained](#8-key-patterns-explained)
9. [File Structure](#9-file-structure)

---

## 1. Big Picture

```
┌─────────────────────────────────────────────────────────┐
│                    USER'S BROWSER                       │
│                                                         │
│  ┌───────────────────────────────────────────────────┐  │
│  │            Angular 22 SPA (port 4200)             │  │
│  │                                                   │  │
│  │  ┌─────────┐  ┌─────────┐  ┌──────────────────┐  │  │
│  │  │  Login  │  │ Workout │  │  Progress Charts │  │  │
│  │  │  Page   │  │  Form   │  │  (Chart.js)      │  │  │
│  │  └────┬────┘  └────┬────┘  └────────┬─────────┘  │  │
│  │       │             │                │             │  │
│  │       └─────────────┼────────────────┘             │  │
│  │                     │                              │  │
│  │              HTTP calls with                       │  │
│  │              JWT Bearer token                      │  │
│  └─────────────────────┼─────────────────────────────┘  │
│                        │                                │
└────────────────────────┼────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────┐
│          ASP.NET Web API (port 5000)                    │
│                                                         │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐              │
│  │  Auth    │  │Workouts  │  │Dashboard │  ... 7 total │
│  │Controller│  │Controller│  │Controller│              │
│  └────┬─────┘  └────┬─────┘  └────┬─────┘              │
│       │             │             │                      │
│       └─────────────┼─────────────┘                      │
│                     │                                    │
│            ┌────────┴────────┐                           │
│            │  EF Core ORM    │                           │
│            └────────┬────────┘                           │
│                     │                                    │
└─────────────────────┼───────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────┐
│              PostgreSQL (port 5432)                      │
│              Database: gym_tracker                       │
│                                                         │
│  Tables: Users, Exercises, Workouts, WorkoutExercises,  │
│          Presets, PresetExercises, DashboardCharts,      │
│          UserSettings + ASP.NET Identity tables          │
└─────────────────────────────────────────────────────────┘
```

---

## 2. How the Pieces Connect

### Request lifecycle

1. User clicks a button in the Angular app
2. A component calls a service method (e.g., `workoutService.create(...)`)
3. The service makes an HTTP request to `http://127.0.0.1:5000/api/workouts`
4. Angular's HTTP interceptor **automatically attaches the JWT token** to the header
5. The request hits the ASP.NET backend
6. The `[Authorize]` attribute checks the JWT token — if invalid/missing, returns 401
7. The controller extracts the user ID from the token's claims
8. The controller calls EF Core to query/modify PostgreSQL
9. The response flows back: PostgreSQL → EF Core → Controller → HTTP response → Angular service → Component → Template updates

### Development servers

```
Angular dev server:  http://localhost:4200  (Vite, hot reload)
ASP.NET API:         http://localhost:5000  (Kestrel)
PostgreSQL:          localhost:5432
```

Angular's `proxy.conf.json` forwards `/api/*` requests to `http://localhost:5000`, so the frontend code just calls `/api/...` without worrying about CORS during development.

---

## 3. Backend (ASP.NET)

### What is ASP.NET?

ASP.NET is Microsoft's framework for building web APIs. Think of it as a engine that:
- Receives HTTP requests (GET, POST, PUT, DELETE)
- Routes them to the right code (controller methods)
- Handles authentication, serialization, database access
- Sends back JSON responses

### Program.cs — The entry point

This is the first file that runs. It does two things:

**A) Register services** (what the app can use):
```
AddControllers()         → Yes, we have controllers
AddDbContext<Postgres>   → We use PostgreSQL as our database
AddIdentity<User>        → We use ASP.NET's user management system
AddJwtBearer()           → We authenticate with JWT tokens
AddCors("AllowAngular")  → Allow requests from localhost:4200
AddScoped<JwtService>    → JWT token generation
AddScoped<ChartService>  → Chart data computation
```

**B) Build the middleware pipeline** (order matters!):
```
Request comes in
  → Swagger (dev only, API docs at /swagger)
  → CORS (check if request origin is allowed)
  → Authentication (decode JWT token, identify user)
  → Authorization (check if user has permission)
  → Route to controller
  → Send response
```

### Controllers — The API endpoints

Each controller is a class that handles HTTP requests for one domain:

```
AuthController      →  /api/auth/*       (login, register)
UserController      →  /api/user/*       (profile, settings)
ExercisesController →  /api/exercises/*  (CRUD for exercises)
WorkoutsController  →  /api/workouts/*   (CRUD for workouts)
PresetsController   →  /api/presets/*    (CRUD for workout templates)
StatsController     →  /api/stats/*      (aggregated statistics)
DashboardController →  /api/dashboard/*  (chart configurations)
```

**How a controller method works (example):**

```csharp
[HttpPost]                              // This handles POST requests
[Authorize]                             // User must be logged in
public async Task<IActionResult> Create([FromBody] CreateWorkoutRequest request)
{
    var userId = int.Parse(_userManager.GetUserId(User)!);  // Get user from JWT
    
    var workout = new Workout                                // Create database record
    {
        UserId = userId,
        Date = request.Date,
        Notes = request.Notes
    };
    
    _context.Workouts.Add(workout);                          // Add to EF Core
    await _context.SaveChangesAsync();                       // Save to PostgreSQL
    
    return CreatedAtAction(...);                             // Return 201 Created
}
```

**Key concepts:**
- `[HttpPost]` / `[HttpGet]` / `[HttpPut]` / `[HttpDelete]` — which HTTP method this handles
- `[Authorize]` — requires a valid JWT token
- `[FromBody]` — the request body is deserialized into this object
- `async/await` — the database call is asynchronous (doesn't block the server)
- `_context` — the EF Core database connection

### Services — Reusable logic

**JwtService:** Generates JWT tokens when users register or login.

```csharp
public string GenerateToken(int userId, string username, string email)
{
    // Create claims (data to store in the token)
    var claims = new[] {
        new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
        new Claim(ClaimTypes.Name, username),
        new Claim(ClaimTypes.Email, email)
    };
    
    // Create token with expiration (60 minutes)
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
    var token = new JwtSecurityToken(
        issuer: "GymTracker",
        audience: "GymTracker",
        expires: DateTime.UtcNow.AddMinutes(60),
        signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
    );
    
    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

**ChartService:** Computes chart data for the progress dashboard.

```csharp
// Given a metric and workout data, compute daily data points
public List<ChartDataPoint> ComputePoints(List<WorkoutExercise> workouts, string metric)
{
    return metric switch
    {
        "weight"    => /* max weight per day */,
        "volume"    => /* total volume per day */,
        "est1rm"    => /* estimated 1RM per day */,
        "reps"      => /* total reps per day */,
        "bodyWeight"=> /* body weight per day */,
        "frequency" => /* workouts per week */,
    };
}
```

### DTOs (Data Transfer Objects)

DTOs are classes that define the shape of API request/response data. They're separate from database models:

```csharp
// What the client sends:
public class CreateWorkoutRequest
{
    public DateTime Date { get; set; }
    public string? Notes { get; set; }
    public decimal? BodyWeight { get; set; }
    public List<WorkoutExerciseRequest> Exercises { get; set; }
}

// What's stored in the database:
public class Workout
{
    public int Id { get; set; }           // Auto-generated
    public int UserId { get; set; }       // Set from JWT
    public ICollection<WorkoutExercise> WorkoutExercises { get; set; }  // Navigation property
}
```

The controller transforms between these two shapes.

---

## 4. Frontend (Angular)

### What is Angular?

Angular is a TypeScript framework for building single-page applications (SPAs). Instead of loading separate HTML pages for each screen, Angular loads one HTML page and dynamically swaps content using JavaScript.

### Key Angular concepts

**Components:** Reusable UI pieces. Each component has:
- A TypeScript class (logic)
- An HTML template (markup)
- Optional CSS (styling)

```typescript
@Component({
  selector: 'app-exercise-list',      // Used in HTML as <app-exercise-list>
  standalone: true,                    // No NgModule needed (modern pattern)
  templateUrl: './exercise-list.html', // The HTML template
})
export class ExerciseList implements OnInit {
  exercises: Exercise[] = [];          // Component state
  
  constructor(private exerciseService: ExerciseService) {}  // Inject dependencies
  
  ngOnInit() {                        // Runs when component loads
    this.exerciseService.getAll().subscribe(exercises => {
      this.exercises = exercises;      // Update state
    });
  }
}
```

**Services:** Singleton classes that handle data fetching and business logic. They're injected into components:

```typescript
@Injectable({ providedIn: 'root' })  // Singleton — one instance shared everywhere
export class WorkoutService {
  private apiUrl = '/api/workouts';
  
  constructor(private http: HttpClient) {}  // Angular's HTTP client
  
  getAll(): Observable<Workout[]> {
    return this.http.get<Workout[]>(this.apiUrl);  // Returns an Observable
  }
}
```

**Routing:** Maps URLs to components:

```typescript
const routes: Routes = [
  { path: 'workouts', loadComponent: () => import('./workout-list').then(m => m.WorkoutList) },
  { path: 'workouts/new', loadComponent: () => import('./workout-form').then(m => m.WorkoutForm) },
  { path: 'workouts/:id/edit', loadComponent: () => import('./workout-form').then(m => m.WorkoutForm) },
];
```

The `:id` part is a URL parameter — the `WorkoutForm` component reads it to know if it's creating or editing.

**Lazy loading:** `loadComponent: () => import(...)` means the JavaScript for that component is only downloaded when the user navigates to it. This makes the initial page load faster.

### How the auth interceptor works

Every HTTP request from Angular passes through the interceptor, which automatically adds the JWT token:

```typescript
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  
  // Skip auth for login/register
  if (req.url.includes('/api/auth/')) {
    return next(req);
  }
  
  const token = authService.getToken();
  if (token) {
    // Clone the request and add the Authorization header
    req = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    });
  }
  
  return next(req).pipe(
    catchError(err => {
      if (err.status === 401) {
        authService.logout();  // Token expired — log out
      }
      return throwError(err);
    })
  );
};
```

### How change detection works

Angular automatically updates the HTML when component state changes. But async operations (HTTP calls) happen outside Angular's awareness. So we manually tell Angular to re-check:

```typescript
this.workoutService.getAll().subscribe({
  next: (workouts) => {
    this.workouts = workouts;
    this.cdr.markForCheck();  // ← "Hey Angular, re-render the template"
  }
});
```

Every component uses this pattern. Without `markForCheck()`, the template would show stale data.

### How charts work

The progress page uses **Chart.js** (a JavaScript charting library) through **ng2-charts** (an Angular wrapper):

```html
<!-- In chart-tile.html -->
<canvas baseChart
        [type]="getChartType()"      <!-- 'line' or 'bar' -->
        [data]="chartData"           <!-- labels + datasets -->
        [options]="chartOptions">    <!-- colors, axes, etc. -->
</canvas>
```

The data flow:
1. Backend computes data points (date/value pairs) and returns them as JSON
2. Angular component maps them into Chart.js format:
   ```
   { labels: ['07-20', '07-21', '07-22'], 
     datasets: [{ data: [80, 82, 85] }] }
   ```
3. Chart.js renders a line/bar chart on the canvas element

---

## 5. Database

### Entity Relationship Diagram

```
┌──────────┐     ┌──────────────┐     ┌──────────┐
│   User   │────▶│ UserSettings │     │ Exercise │
│          │     │ (1:1)        │     │          │
│ Id       │     │ Theme        │     │ Id       │
│ UserName │     │ RestTimer    │     │ Name ★   │
│ Email    │     └──────────────┘     │ MuscleGrp│
│ Weight   │                          │ IsDuration│
│ Height   │                          │ IsDefault │
└────┬─────┘                          └────┬─────┘
     │                                     │
     │ 1:*                                 │ *:1
     │                                     │
┌────┴──────┐     ┌──────────────────┐    │
│  Workout  │────▶│ WorkoutExercise  │────┘
│           │     │                  │
│ Date      │     │ Sets, Reps       │
│ Notes     │     │ Weight, Duration │
│ BodyWeight│     │ RestTime         │
└───────────┘     └──────────────────┘

┌──────────┐     ┌──────────────────┐
│  Preset  │────▶│ PresetExercise   │────▶ Exercise
│          │     │                  │
│ Name     │     │ DefaultSets      │
│          │     │ DefaultReps      │
│          │     │ DefaultWeight    │
└──────────┘     └──────────────────┘

┌──────────────────┐
│ DashboardChart   │────▶ Exercise (optional)
│                  │
│ Label, Metric    │
│ Period, ChartType│
│ Position         │
└──────────────────┘
```

### Why this design?

**Why join tables (WorkoutExercise, PresetExercise)?**
A workout contains multiple exercises, each with different sets/reps/weight. This is a many-to-many relationship with extra data — perfect for a join table.

**Why UserSettings as a separate table?**
Keeps preferences isolated from user identity. Can be extended without touching the User table.

**Why DashboardChart stores config, not data?**
Charts are computed on-the-fly from workout data. Storing the config (metric, period, exercise) lets users customize charts without us pre-computing and storing results.

### How EF Core works

Entity Framework Core is an **Object-Relational Mapper (ORM)**. It maps C# classes to database tables:

```csharp
// This C# class...
public class Workout {
    public int Id { get; set; }
    public string? Notes { get; set; }
    public ICollection<WorkoutExercise> WorkoutExercises { get; set; }
}

// ...maps to this SQL table:
// CREATE TABLE "Workouts" (
//     "Id" SERIAL PRIMARY KEY,
//     "Notes" VARCHAR(500),
//     "UserId" INTEGER REFERENCES "Users"("Id")
// );
```

When you write `_context.Workouts.Add(workout)`, EF Core generates the INSERT SQL automatically.

### Migrations

Migrations track database schema changes. When you add a new model property:
```
dotnet ef migrations add AddDashboardCharts
dotnet ef database update
```

EF Core generates C# code that describes the schema change, and applies it to the database. On app startup, `context.Database.Migrate()` applies any pending migrations automatically.

---

## 6. Authentication

### The complete auth flow

```
REGISTER:
1. User fills form: username, email, password
2. POST /api/auth/register with form data
3. Backend: create User record (password is hashed with PBKDF2)
4. Backend: create UserSettings with defaults
5. Backend: generate JWT token with userId, username, email
6. Return token to frontend
7. Frontend: store token in localStorage
8. Frontend: redirect to /workouts

LOGIN:
1. User enters email + password
2. POST /api/auth/login
3. Backend: find user by email, verify password hash
4. Backend: generate JWT token
5. Return token
6. Frontend: store token, redirect to /workouts

SUBSEQUENT REQUESTS:
1. Frontend interceptor reads token from localStorage
2. Adds header: Authorization: Bearer <token>
3. Backend validates token signature and expiration
4. If valid, extracts userId from token claims
5. Controller processes request for that user
```

### What's in a JWT token?

```
Header:    { "alg": "HS256" }
Payload:   { "nameid": "42",          ← user ID
              "name": "john",          ← username
              "email": "john@example.com",
              "exp": 1722000000,       ← expiration (60 min)
              "iss": "GymTracker" }    ← who issued it
Signature: HMAC-SHA256(header + payload, secret_key)
```

The token is **signed**, not encrypted. Anyone can read the payload, but nobody can forge it without the secret key.

### Why JWT?

- **Stateless:** The server doesn't need to store sessions. The token contains all needed info.
- **Scalable:** Multiple server instances can all validate the same token.
- **Standard:** Works across different languages/frameworks.

### Password hashing

Passwords are never stored in plain text. ASP.NET Identity uses PBKDF2 (Password-Based Key Derivation Function 2):
```
Plain password: "Admin123!"
     ↓ PBKDF2 (600,000 iterations + salt)
Hashed password: "AQAAAAEAACcQAAAAENx..."
```

Even if someone steals the database, they can't reverse the hashes to get passwords.

---

## 7. API Endpoints

### Auth (no login required)

| Method | Endpoint | Body | Response |
|--------|----------|------|----------|
| POST | `/api/auth/register` | `{ username, email, password, weight?, height? }` | `{ token, userId }` |
| POST | `/api/auth/login` | `{ email, password }` | `{ token, userId }` |

### Exercises

| Method | Endpoint | Body | Response |
|--------|----------|------|----------|
| GET | `/api/exercises` | — | `[{ id, name, muscleGroup, isDuration, isDefault }]` |
| GET | `/api/exercises/:id` | — | `{ id, name, ... }` |
| POST | `/api/exercises` | `{ name, muscleGroup, isDuration }` | Created exercise |
| PUT | `/api/exercises/:id` | `{ name, muscleGroup, isDuration }` | Updated exercise |
| DELETE | `/api/exercises/:id` | — | 204 No Content |

### Workouts

| Method | Endpoint | Body | Response |
|--------|----------|------|----------|
| GET | `/api/workouts` | — | `[{ id, date, notes, bodyWeight, workoutExercises }]` |
| GET | `/api/workouts/:id` | — | Single workout |
| POST | `/api/workouts` | `{ date, notes, bodyWeight, exercises: [...] }` | Created workout |
| POST | `/api/workouts/from-preset/:presetId` | — | Workout created from preset |
| PUT | `/api/workouts/:id` | `{ date, notes, bodyWeight, exercises: [...] }` | Updated workout |
| DELETE | `/api/workouts/:id` | — | 204 |

### Presets

| Method | Endpoint | Body | Response |
|--------|----------|------|----------|
| GET | `/api/presets` | — | `[{ id, name, presetExercises }]` |
| POST | `/api/presets` | `{ name, exercises: [...] }` | Created preset |
| PUT | `/api/presets/:id` | `{ name, exercises: [...] }` | Updated preset |
| DELETE | `/api/presets/:id` | — | 204 |

### Dashboard

| Method | Endpoint | Body | Response |
|--------|----------|------|----------|
| GET | `/api/dashboard` | — | `[{ id, label, metric, period, chartType, data: { points, summary } }]` |
| POST | `/api/dashboard` | `{ label, metric, exerciseId?, period, chartType }` | Created chart with data |
| PUT | `/api/dashboard/:id` | `{ label, metric, exerciseId?, period, chartType }` | Updated chart |
| DELETE | `/api/dashboard/:id` | — | 204 |
| PUT | `/api/dashboard/reorder` | `[{ id, position }]` | 200 |

### Stats

| Method | Endpoint | Body | Response |
|--------|----------|------|----------|
| GET | `/api/stats` | — | Aggregated stats |
| POST | `/api/stats/chart-data` | `{ metric, exerciseId?, period }` | `{ points, summary }` |

---

## 8. Key Patterns Explained

### Pattern: Delete-and-reinsert for nested collections

When updating a workout's exercises, the backend doesn't try to figure out which exercises were added/removed/modified. It just deletes all existing `WorkoutExercise` rows and inserts the new ones:

```csharp
// Remove old exercises
var existing = _context.WorkoutExercises.Where(we => we.WorkoutId == id);
_context.WorkoutExercises.RemoveRange(existing);

// Add new exercises
foreach (var e in request.Exercises) {
    workout.WorkoutExercises.Add(new WorkoutExercise { ... });
}
await _context.SaveChangesAsync();
```

**Why?** It's simpler and avoids complex diffing logic. For a gym app with ~10 exercises per workout, performance is not a concern.

### Pattern: Profile weight auto-sync

When a workout is created or updated with a body weight, the backend finds the most recent workout by date and syncs that weight to the user's profile:

```csharp
private async Task SyncProfileWeight(int userId)
{
    var latestWeight = await _context.Workouts
        .Where(w => w.UserId == userId && w.BodyWeight.HasValue)
        .OrderByDescending(w => w.Date)     // Most recent by USER-SET date
        .Select(w => w.BodyWeight)
        .FirstOrDefaultAsync();
    
    user.Weight = latestWeight;
}
```

**Why by date, not insertion order?** Users can log workouts for past dates. The profile should show the weight from the most recent workout date, not the most recently created record.

### Pattern: Chart computation on-the-fly

Charts aren't pre-computed and stored. Every time the dashboard loads, the backend:
1. Queries all relevant `WorkoutExercise` records
2. Groups them by day
3. Computes the metric (weight, volume, est1RM, etc.)
4. Returns data points + summary

**Why?** Fresh data every time. No stale cache to invalidate. The dataset is small enough (one user's workouts) that computation is instant.

### Pattern: Functional guards and interceptors

Modern Angular uses plain functions instead of classes for guards and interceptors:

```typescript
// Old way (Angular < 15):
@Injectable() class AuthGuard implements CanActivate { ... }

// New way (Angular 15+):
export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  return authService.isLoggedIn() ? true : inject(Router).createUrlTree(['/login']);
};
```

**Why?** Simpler, less boilerplate, works better with tree-shaking.

### Pattern: CSS variables for theming

The entire color scheme is defined in CSS custom properties:

```css
:root {
  --bg: #282828;
  --fg: #ebdbb2;
  --green: #b8bb26;
}

[data-theme="light"] {
  --bg: #fbf1c7;
  --fg: #3c3836;
  --green: #98971a;
}
```

Switching themes is just toggling an attribute on `<html>`. Every element using `var(--bg)` automatically updates.

---

## 9. File Structure

```
backend/
├── Program.cs                    ← Entry point, service registration, middleware
├── appsettings.json              ← Config (DB connection, JWT secret)
├── GymTracker.csproj             ← NuGet packages
├── Controllers/
│   ├── AuthController.cs         ← Login, register
│   ├── UserController.cs         ← Profile, settings
│   ├── ExercisesController.cs    ← Exercise CRUD
│   ├── WorkoutsController.cs     ← Workout CRUD
│   ├── PresetsController.cs      ← Preset CRUD
│   ├── StatsController.cs        ← Aggregated stats
│   └── DashboardController.cs    ← Chart config CRUD
├── Models/
│   ├── User.cs                   ← User entity (extends IdentityUser)
│   ├── UserSettings.cs           ← Theme, preferences
│   ├── Exercise.cs               ← Exercise catalog
│   ├── Workout.cs                ← Workout session
│   ├── WorkoutExercise.cs        ← Join: workout ↔ exercise
│   ├── Preset.cs                 ← Workout template
│   ├── PresetExercise.cs         ← Join: preset ↔ exercise
│   └── DashboardChart.cs         ← Chart configuration
├── Data/
│   ├── GymDbContext.cs           ← EF Core database context
│   └── SeedData.cs               ← 31 default exercises
├── Services/
│   ├── JwtService.cs             ← JWT token generation
│   └── ChartService.cs           ← Chart data computation
└── Migrations/                   ← Database schema history

frontend/
├── src/
│   ├── main.ts                   ← Bootstrap
│   ├── index.html                ← HTML shell
│   ├── styles.css                ← Global Gruvbox theme
│   ├── proxy.conf.json           ← Dev proxy to backend
│   └── app/
│       ├── app.config.ts         ← Providers, Chart.js setup
│       ├── app.routes.ts         ← All routes
│       ├── app.ts / .html / .css ← Root component, nav
│       ├── auth/
│       │   ├── auth.service.ts       ← Login, register, token management
│       │   ├── auth.interceptor.ts   ← Auto-attach JWT to requests
│       │   ├── auth.guard.ts         ← Protect routes
│       │   ├── login/                ← Login page
│       │   └── register/             ← Registration page
│       ├── workouts/
│       │   ├── workout.service.ts    ← Workout API calls
│       │   ├── workout-list/         ← Workout log page
│       │   └── workout-form/         ← Create/edit workout
│       ├── exercises/
│       │   ├── exercise.service.ts   ← Exercise API calls
│       │   ├── exercise-list/        ← Exercise table
│       │   └── exercise-form/        ← Create/edit exercise
│       ├── presets/
│       │   ├── preset.service.ts     ← Preset API calls
│       │   ├── preset-list/          ← Preset cards
│       │   └── preset-form/          ← Create/edit preset
│       ├── progress/
│       │   ├── dashboard.service.ts  ← Dashboard API calls
│       │   ├── progress-page.ts      ← Dashboard grid
│       │   ├── chart-tile.ts         ← Single chart card
│       │   └── chart-editor.ts       ← Chart config modal
│       └── settings/
│           ├── settings.service.ts   ← Profile/settings API
│           └── settings/             ← Profile, theme, logout
```

---

## Quick Reference

### To add a new API endpoint:
1. Add a model class in `Models/`
2. Add a DbSet in `GymDbContext.cs`
3. Create a migration: `dotnet ef migrations add <Name>`
4. Create a controller (or add to existing) in `Controllers/`
5. Register any new services in `Program.cs`

### To add a new Angular page:
1. Create component files (`.ts`, `.html`, `.css`)
2. Add a route in `app.routes.ts`
3. Create a service method for API calls
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
