## ADDED Requirements

### Requirement: Add language field to UserSettings
The system SHALL add a `Language` column to the `UserSettings` entity and expose it through the API.

#### Scenario: UserSettings includes language
- **WHEN** the `UserSettings` entity is queried
- **THEN** the `Language` field SHALL be returned with values `kz`, `ru`, or `en`

### Requirement: Validate language values
The system SHALL accept only supported language values when updating settings.

#### Scenario: Valid language update
- **WHEN** a user sends a settings update with `language` set to `kz`, `ru`, or `en`
- **THEN** the system SHALL save the value and return the updated settings

#### Scenario: Invalid language update
- **WHEN** a user sends a settings update with `language` set to any other value
- **THEN** the system SHALL reject the request with a 400 status

### Requirement: Default language for new users
The system SHALL set a default language for new users during registration based on the same detection logic used for first-time visitors.

#### Scenario: New user registration
- **WHEN** a new user registers
- **THEN** the system SHALL create `UserSettings` with a default language determined by browser locale or IP geolocation

### Requirement: Migrate existing user settings
The system SHALL add a migration for the new `Language` column and set existing rows to a default value.

#### Scenario: Existing rows without language
- **WHEN** the migration runs on the existing database
- **THEN** all existing `UserSettings` rows SHALL receive a default language of `en`
