## ADDED Requirements

### Requirement: Onboarding guide SHALL be shown to new users
An onboarding guide component SHALL be displayed on the `/workouts` page after a user's first login. The guide SHALL show until the user explicitly dismisses it.

#### Scenario: First-time user sees guide
- **WHEN** a user logs in for the first time and navigates to /workouts
- **THEN** an onboarding guide box is visible on the page

#### Scenario: Returning user does not see guide
- **WHEN** a user has previously dismissed the guide and visits /workouts
- **THEN** the onboarding guide is not shown

### Requirement: Onboarding guide SHALL contain 3-step instructions
The guide SHALL display three steps:
1. Create a Preset (link to Settings > Manage Presets)
2. Log a Workout (link to New Workout)
3. Track Progress (link to Progress page)

Each step SHALL have a brief description and a clickable action.

#### Scenario: Guide displays three steps
- **WHEN** the onboarding guide is rendered
- **THEN** it shows exactly 3 numbered steps with descriptions

#### Scenario: Step links navigate correctly
- **WHEN** a user clicks the "Log a Workout" action in the guide
- **THEN** they are navigated to /workouts/new

### Requirement: Onboarding guide SHALL be dismissible
The guide SHALL have a "Got it" button that, when clicked, permanently hides the guide. Dismissal SHALL be stored in localStorage.

#### Scenario: User dismisses guide
- **WHEN** a user clicks "Got it" on the onboarding guide
- **THEN** the guide disappears and `localStorage.onboardingDismissed` is set to `"true"`

#### Scenario: Dismissal persists across sessions
- **WHEN** a user dismisses the guide and refreshes the page
- **THEN** the guide does not reappear

### Requirement: Onboarding guide SHALL match TUI aesthetic
The guide SHALL use the app's existing Gruvbox theme variables and monospace font. It SHALL be a styled instruction card, not a spotlight/overlay tour.

#### Scenario: Guide uses theme variables
- **WHEN** the onboarding guide is rendered
- **THEN** it uses `var(--bg)`, `var(--fg)`, `var(--border)` and other theme variables for styling
