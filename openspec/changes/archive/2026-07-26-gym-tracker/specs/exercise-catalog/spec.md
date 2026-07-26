## ADDED Requirements

### Requirement: List exercises
The system SHALL return all exercises including pre-loaded defaults and user-created ones.

#### Scenario: Retrieve exercise list
- **WHEN** user requests exercise list
- **THEN** system returns all exercises with name, muscleGroup, isDuration

### Requirement: Get single exercise
The system SHALL return a specific exercise by ID.

#### Scenario: Retrieve exercise
- **WHEN** user requests exercise with ID 1
- **THEN** system returns exercise with all fields

### Requirement: Create custom exercise
The system SHALL allow users to create custom exercises.

#### Scenario: Create exercise
- **WHEN** user creates exercise with name "My Custom Exercise", muscleGroup "Back"
- **THEN** system creates exercise with isDefault=false

#### Scenario: Duplicate exercise name
- **WHEN** user creates exercise with existing name
- **THEN** system returns error "Exercise name already exists"

### Requirement: Update exercise
The system SHALL allow users to modify custom exercises.

#### Scenario: Update custom exercise
- **WHEN** user updates their custom exercise
- **THEN** system saves changes

#### Scenario: Update default exercise
- **WHEN** user tries to modify a pre-loaded default exercise
- **THEN** system returns error "Cannot modify default exercises"

### Requirement: Delete exercise
The system SHALL allow users to delete custom exercises not in use.

#### Scenario: Delete unused custom exercise
- **WHEN** user deletes custom exercise with no workout associations
- **THEN** system removes exercise

#### Scenario: Delete default exercise
- **WHEN** user tries to delete a pre-loaded default exercise
- **THEN** system returns error "Cannot delete default exercises"

#### Scenario: Delete exercise in use
- **WHEN** user deletes exercise linked to workouts or presets
- **THEN** system returns error "Exercise is in use"
