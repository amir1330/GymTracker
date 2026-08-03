## ADDED Requirements

### Requirement: Detect default language from browser locale
The system SHALL determine the initial language from the browser's preferred language when it matches one of the supported languages.

#### Scenario: Browser locale is supported
- **WHEN** a user visits the app for the first time with a browser preferred language of `kk`, `ru`, or `en`
- **THEN** the system SHALL set the active language to the matching supported language (`kz`, `ru`, or `en`)

#### Scenario: Browser locale is an unsupported variant
- **WHEN** a browser preferred language is a variant of a supported language (e.g., `en-US` or `ru-KZ`)
- **THEN** the system SHALL map the language portion to the supported language and ignore the region

#### Scenario: Browser locale is not supported
- **WHEN** a browser preferred language does not match any supported language
- **THEN** the system SHALL fall back to the IP geolocation default and only use English as the final fallback

### Requirement: Fall back to IP geolocation default
The system SHALL infer a default language from the user's IP location when the browser locale is ambiguous or unsupported.

#### Scenario: IP resolves to Kazakhstan
- **WHEN** the browser locale is unsupported and the IP geolocation resolves to Kazakhstan
- **THEN** the system SHALL default to Kazakh (`kz`)

#### Scenario: IP resolves to Russia
- **WHEN** the browser locale is unsupported and the IP geolocation resolves to Russia
- **THEN** the system SHALL default to Russian (`ru`)

#### Scenario: IP resolves to other country
- **WHEN** the browser locale is unsupported and the IP geolocation resolves to any other country
- **THEN** the system SHALL default to English (`en`)

### Requirement: Skip detection when user already has a saved preference
The system SHALL not override a language preference that has already been saved for the user.

#### Scenario: Authenticated user has saved language
- **WHEN** a logged-in user has a saved language preference in `UserSettings`
- **THEN** the system SHALL use the saved preference and skip locale and IP detection

#### Scenario: Guest user has a previous selection
- **WHEN** a language has been selected and stored in the browser during the current session or local storage
- **THEN** the system SHALL use that stored language and skip locale and IP detection
