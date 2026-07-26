## ADDED Requirements

### Requirement: List presets
The system SHALL return all workout presets for the authenticated user.

#### Scenario: Retrieve presets
- **WHEN** user requests preset list
- **THEN** system returns all user's presets with exercises

### Requirement: Create preset
The system SHALL allow users to create workout presets with exercises.

#### Scenario: Create preset
- **WHEN** user creates preset "Push Day" with exercises Bench Press (3x10x135) and Overhead Press (3x8x95)
- **THEN** system creates preset with preset exercises

### Requirement: Update preset
The system SHALL allow users to modify preset name and exercises.

#### Scenario: Update preset exercises
- **WHEN** user adds exercise to preset
- **THEN** system updates preset with new exercise

### Requirement: Delete preset
The system SHALL allow users to remove presets.

#### Scenario: Delete preset
- **WHEN** user deletes preset
- **THEN** system removes preset and its exercises

### Requirement: Create workout from preset
The system SHALL allow users to create a workout log from a preset.

#### Scenario: Create workout from preset
- **WHEN** user selects preset "Push Day" to create workout
- **THEN** system creates workout with exercises copied from preset with default values
