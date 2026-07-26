## ADDED Requirements

### Requirement: List workouts
The system SHALL return all workouts for the authenticated user sorted by date descending.

#### Scenario: Retrieve workouts
- **WHEN** user requests workout list
- **THEN** system returns all workouts ordered by date (newest first)

### Requirement: Get single workout
The system SHALL return a specific workout with all exercise details.

#### Scenario: Retrieve workout
- **WHEN** user requests workout with ID 1
- **THEN** system returns workout with exercises, sets, reps, weight, duration, restTime

### Requirement: Create workout
The system SHALL allow users to create a workout with exercises.

#### Scenario: Create workout from scratch
- **WHEN** user creates workout with date, notes, bodyWeight, and exercises
- **THEN** system creates workout with workout exercises

#### Scenario: Create workout from preset
- **WHEN** user creates workout from preset "Push Day"
- **THEN** system creates workout with preset's exercises and default values

### Requirement: Update workout
The system SHALL allow users to modify workout details and exercises.

#### Scenario: Update workout notes
- **WHEN** user updates workout notes
- **THEN** system saves changes

#### Scenario: Update exercise performance
- **WHEN** user updates exercise reps from 10 to 8
- **THEN** system saves the actual performance

### Requirement: Delete workout
The system SHALL allow users to remove workouts.

#### Scenario: Delete workout
- **WHEN** user deletes workout
- **THEN** system removes workout and all workout exercises

### Requirement: Add exercise to workout
The system SHALL allow users to add exercises to an existing workout.

#### Scenario: Add exercise
- **WHEN** user adds exercise to workout
- **THEN** system creates workout exercise record

### Requirement: Remove exercise from workout
The system SHALL allow users to remove exercises from a workout.

#### Scenario: Remove exercise
- **WHEN** user removes exercise from workout
- **THEN** system deletes workout exercise record
