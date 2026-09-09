## ADDED Requirements

### Requirement: Simple subscription pattern
All HTTP subscriptions SHALL use the simple callback form `.subscribe(data => ...)` instead of the object form `{ next: ..., error: ... }`.

#### Scenario: Page loads data
- **WHEN** a page component fetches data from a service
- **THEN** the subscription SHALL use `.subscribe(data => { ... })` without explicit `next`/`error` separation

#### Scenario: Form submission
- **WHEN** a form component submits data
- **THEN** the subscription SHALL use `.subscribe(data => { ... })` pattern

### Requirement: No premature error handling
Explicit error handlers in subscriptions SHALL NOT be used unless error display is a user-facing requirement. Global error handling (interceptor) handles unexpected errors.

#### Scenario: Delete operation
- **WHEN** a user deletes an item
- **THEN** the subscription SHALL NOT include an explicit `error` callback

## REMOVED Requirements

### Requirement: Explicit per-subscription error handling
**Reason**: Premature optimization. The HTTP interceptor already handles 401 errors globally. Other errors are not user-recoverable and don't need per-subscription handlers.
**Migration**: Remove all `{ next: ..., error: ... }` object subscriptions. Use `.subscribe(data => ...)` form.
