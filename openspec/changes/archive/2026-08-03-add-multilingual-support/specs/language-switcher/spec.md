## ADDED Requirements

### Requirement: Display language selector in settings
The system SHALL provide a language selector in the Settings page that allows the user to switch between Kazakh, Russian, and English.

#### Scenario: Settings page shows language selector
- **WHEN** the user navigates to the Settings page
- **THEN** the system SHALL display the current language and a control to select a different language

#### Scenario: Selecting a language updates the UI immediately
- **WHEN** the user selects a language from the selector
- **THEN** the system SHALL immediately apply the selected language to all translated strings in the UI

### Requirement: Persist language preference
The system SHALL save the user's selected language to the backend when the user is authenticated.

#### Scenario: Authenticated user changes language
- **WHEN** an authenticated user changes the language in settings
- **THEN** the system SHALL send the updated `language` value to the backend settings endpoint
- **AND** the system SHALL save the selection to the backend's `UserSettings` table

#### Scenario: Guest user changes language
- **WHEN** a guest user changes the language in settings
- **THEN** the system SHALL store the language in browser storage so it persists across page reloads

### Requirement: Supported languages
The system SHALL support exactly three languages: Kazakh (`kz`), Russian (`ru`), and English (`en`).

#### Scenario: Language selector options
- **WHEN** the language selector is rendered
- **THEN** the system SHALL display the options `Қазақша`, `Русский`, and `English` mapped to `kz`, `ru`, and `en`
