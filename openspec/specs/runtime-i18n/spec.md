## ADDED Requirements

### Requirement: Use a runtime translation library
The system SHALL use a runtime translation library that supports lazy-loaded JSON translation files and immediate language switching without reloading the page.

#### Scenario: Translation library is registered
- **WHEN** the Angular application is bootstrapped
- **THEN** the system SHALL register the translation library, HTTP loader, and default language

#### Scenario: Translation files are loaded on demand
- **WHEN** the active language changes
- **THEN** the system SHALL load the corresponding JSON translation file via HTTP and update the UI

### Requirement: Translate all user-facing strings
The system SHALL replace every hard-coded user-facing string in the UI with a translation key that is present in all three translation files.

#### Scenario: Page titles and navigation are translated
- **WHEN** the user views pages and navigation
- **THEN** every title, button, label, link, and navigation item SHALL use translation keys

#### Scenario: Form labels and validation messages are translated
- **WHEN** the user views forms
- **THEN** all labels, placeholders, helper text, and validation messages SHALL use translation keys

#### Scenario: Empty/error states are translated
- **WHEN** the user sees empty lists, error messages, or confirmation dialogs
- **THEN** all such strings SHALL use translation keys

### Requirement: Provide translation keys for programmatic use
The system SHALL provide a service method that returns translated strings for messages generated in TypeScript code.

#### Scenario: Programmatic translation
- **WHEN** a component or service needs to display a translated string from code
- **THEN** it SHALL call the translation service to get the translated value for the current language

### Requirement: Keep translation files organized
The system SHALL organize translation files by language and keep them in a dedicated directory.

#### Scenario: Translation file structure
- **WHEN** translation files are added
- **THEN** they SHALL be placed under `src/assets/i18n/` with filenames `kz.json`, `ru.json`, and `en.json`
