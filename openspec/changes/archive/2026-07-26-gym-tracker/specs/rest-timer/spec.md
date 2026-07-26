## ADDED Requirements

### Requirement: Toggle rest timer
The system SHALL allow users to enable or disable the rest timer in settings.

#### Scenario: Enable timer
- **WHEN** user enables rest timer in settings
- **THEN** system shows timer during workouts

#### Scenario: Disable timer
- **WHEN** user disables rest timer in settings
- **THEN** system hides timer during workouts

### Requirement: Set rest duration
The system SHALL allow users to set default rest time between sets.

#### Scenario: Set default rest time
- **WHEN** user sets rest time to 90 seconds
- **THEN** system uses 90 seconds as default timer duration

### Requirement: Track rest time per exercise
The system SHALL optionally record rest time between sets in workout logs.

#### Scenario: Record rest time
- **WHEN** user completes a set and starts timer
- **THEN** system records rest time when timer completes or is skipped
