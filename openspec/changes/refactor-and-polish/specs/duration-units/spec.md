## ADDED Requirements

### Requirement: Exercise model SHALL have a DurationUnit field
The Exercise entity SHALL have a `DurationUnit` field of type enum `DurationUnit` with values `Seconds`, `Minutes`, `Hours`. The default value SHALL be `Seconds`.

#### Scenario: Exercise created with duration unit
- **WHEN** a user creates a duration-based exercise with unit "minutes"
- **THEN** the Exercise record stores `DurationUnit = Minutes`

#### Scenario: Exercise created without duration (reps-based)
- **WHEN** a user creates a reps-based exercise
- **THEN** the Exercise record stores `DurationUnit = Seconds` (default, unused)

### Requirement: WorkoutExercise SHALL inherit duration unit from Exercise
When a WorkoutExercise is created, its `DurationUnit` SHALL be copied from the linked Exercise. This preserves the unit at the time of logging even if the Exercise's unit changes later.

#### Scenario: Workout exercise inherits unit
- **WHEN** a user logs a workout with "Treadmill Run" (DurationUnit=Minutes)
- **THEN** the WorkoutExercise stores `DurationUnit = Minutes`

#### Scenario: Workout exercise unit persists independently
- **WHEN** an Exercise's DurationUnit is changed after a workout was logged
- **THEN** the existing WorkoutExercise retains its original DurationUnit

### Requirement: Exercise form SHALL display unit selector
The exercise creation/edit form SHALL show a unit dropdown (Seconds/Minutes/Hours) when the "Duration-based" checkbox is checked. The dropdown SHALL be hidden for reps-based exercises.

#### Scenario: Duration checkbox checked
- **WHEN** a user checks the "Duration-based" checkbox
- **THEN** a unit dropdown appears with options: Seconds, Minutes, Hours

#### Scenario: Duration checkbox unchecked
- **WHEN** a user unchecks the "Duration-based" checkbox
- **THEN** the unit dropdown is hidden

### Requirement: Workout and preset forms SHALL display unit labels
When logging a duration-based exercise, the input placeholder and display SHALL show the unit label (e.g., "Duration (min)") instead of hardcoded "(s)".

#### Scenario: Workout form shows unit
- **WHEN** a user adds a duration-based exercise with unit "minutes" to a workout
- **THEN** the duration input placeholder shows "Duration (min)"

#### Scenario: Preset form shows unit
- **WHEN** a user adds a duration-based exercise with unit "hours" to a preset
- **THEN** the duration input placeholder shows "Duration (hr)"

### Requirement: Workout and preset lists SHALL display formatted duration
Duration values in workout lists and preset lists SHALL be displayed with the appropriate unit suffix (e.g., "30min", "1.5hr", "45s") instead of raw seconds.

#### Scenario: List displays duration with unit
- **WHEN** a workout contains a duration exercise of 30 minutes
- **THEN** the list displays "(30min)" not "(1800s)"

### Requirement: Charts SHALL support duration metric
The chart system SHALL include a "duration" metric type that tracks duration-based exercises over time. Duration SHALL be normalized to seconds for comparison across exercises with different units.

#### Scenario: Duration chart displays data
- **WHEN** a user adds a chart with metric "duration" for a duration-based exercise
- **THEN** the chart shows duration values over time

#### Scenario: Duration metric excluded for reps exercises
- **WHEN** a user tries to add a "duration" chart for a reps-based exercise
- **THEN** the option is not available or shows no data
