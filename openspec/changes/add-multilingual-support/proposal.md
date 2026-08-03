## Why

GymTracker currently displays the entire UI in English only. Users in Kazakhstan and Russia need the app in Kazakh (`kz`) and Russian (`ru`) as well as English (`en`). Adding multilingual support will make the app accessible to a broader audience and improve the experience for non-English speakers.

## What Changes

- Introduce a runtime internationalization (i18n) layer in the Angular frontend with translation files for Kazakh, Russian, and English.
- Detect the default language on first visit from the browser's locale, falling back to IP-based geolocation when the locale is ambiguous.
- Add a language switcher to the Settings page so users can override the detected/default language at any time.
- Persist the selected language in `UserSettings` via the backend so it is restored across devices/sessions.
- Replace all hard-coded UI strings across pages and components with translation keys.

## Capabilities

### New Capabilities

- `locale-detection`: Detect the default language on first visit. Use the browser's preferred language when it matches `kz`, `ru`, or `en`; otherwise fall back to IP geolocation to infer Kazakhstan/Russia/Other and map to a default.
- `language-switcher`: Provide a language selector in the Settings page. Persist the user's choice to the backend and immediately apply it across the UI.
- `runtime-i18n`: Provide a runtime translation system in Angular with JSON translation files, a pipe/directive for keys, and a service for programmatic translation. Cover all current user-facing strings.
- `user-settings-storage`: Extend `UserSettings` and the settings API to store and return a `language` field. Existing theme and rest timer behavior remains unchanged.

### Modified Capabilities

- None.

## Impact

- **Frontend**: Adds `@ngx-translate/core` and `@ngx-translate/http-loader` (or equivalent runtime i18n library), new `translate/` module/folder, updates to all page/components/templates, and a language selector in the Settings page.
- **Backend**: Adds `Language` property to `UserSettings`, updates `UpdateSettingsRequest`/`UserSettings` mapping, and generates an EF Core migration.
- **Database**: New `Language` column in `UserSettings` table.
- **Deployment**: No infrastructure changes; existing Docker Compose deployment pipeline continues to work.
