## ADDED Requirements

### Requirement: Preset selection in workout form
The workout form SHALL display a row of preset buttons at the top of the form, below the date/notes/bodyweight fields and above the exercise list.

**Scenarios:**
- **When** the user opens the workout form
  - **Then** all user's presets are shown as clickable buttons/chips
- **When** the user clicks a preset button
  - **Then** the exercise list is populated with exercises from that preset
  - **And** each exercise uses defaultSets, defaultReps, defaultWeight, defaultDuration from the preset
  - **And** existing exercises in the form are replaced (not appended)
- **When** the user has no presets
  - **Then** the preset row shows a message like "No presets yet" or is hidden
- **When** the user clicks a different preset after one is already loaded
  - **Then** the exercise list is replaced with the new preset's exercises

### Requirement: Preset does not overwrite manual exercises
If the user has manually added exercises and then clicks a preset, the preset replaces the exercise list. This is intentional — the preset is a "load template" action, not "add to current".
