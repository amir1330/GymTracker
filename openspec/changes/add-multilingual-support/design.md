## Context

GymTracker is a full-stack Angular 22 + ASP.NET 8 gym tracking app. The frontend is a standalone-component Angular SPA using zoneless change detection, and the backend uses EF Core with PostgreSQL. The app already has a Settings page and a `UserSettings` model that stores theme and rest timer. The goal is to add Kazakh, Russian, and English language support while keeping the existing architecture intact.

## Goals / Non-Goals

**Goals:**
- Add runtime i18n with three supported languages (`kz`, `ru`, `en`).
- Detect the default language from the browser locale, falling back to IP geolocation and then English.
- Add a language selector to the Settings page.
- Persist the selected language to `UserSettings` for authenticated users.
- Translate all user-facing strings in the UI.

**Non-Goals:**
- Server-side rendering of translations.
- Right-to-left (RTL) layout support.
- Translating backend error messages or API responses.
- Supporting languages outside of Kazakh, Russian, and English.

## Decisions

### Use `ngx-translate` for runtime translation
**Rationale:** Angular's built-in `$localize` is build-time and requires separate builds per language, which complicates runtime switching. `@ngx-translate/core` with `@ngx-translate/http-loader` supports lazy-loaded JSON files and runtime language changes, fitting the requirement to switch languages in settings.

### Detect locale from browser first, then IP geolocation
**Rationale:** The browser's `navigator.language` is the most reliable source for the user's preference. It covers the common case without any external API call. IP geolocation is only used as a fallback when the browser locale is not one of the supported languages.

### Store language preference in `UserSettings`
**Rationale:** The app already has a settings endpoint and persistence model. Adding a `Language` column is the smallest change and keeps the preference synced across devices for logged-in users.

### Store guest language in `localStorage`
**Rationale:** Guests need persistence across reloads but have no backend account. `localStorage` is sufficient for this single preference and avoids introducing cookies or server-side sessions.

### Use `kz` instead of `kk` for Kazakh code
**Rationale:** The user explicitly requested `kz` and `ru`. The mapping from `kk` (ISO code for Kazakh) to `kz` will be handled at the locale-detection layer so the rest of the system uses the user's preferred codes.

## Risks / Trade-offs

- **Risk:** Some UI strings may be missed during translation key replacement.  
  **Mitigation:** Audit templates and TypeScript code for hard-coded strings; test each page in all three languages.
- **Risk:** IP geolocation introduces an external dependency.  
  **Mitigation:** Make the geolocation service optional and fail gracefully to English if the service is unavailable or slow.
- **Risk:** Existing `UserSettings` rows need a default language.  
  **Mitigation:** Include an EF Core migration that sets `Language` to `en` for existing rows.

## Migration Plan

1. Update backend `UserSettings` model and add `Language` field.
2. Create EF Core migration and apply it to PostgreSQL.
3. Install `ngx-translate` and `http-loader` in the frontend.
4. Add translation files and the translation module/provider.
5. Replace hard-coded strings with translation keys across templates and components.
6. Add locale detection and language selector to Settings.
7. Deploy via existing Docker Compose pipeline; the app will start automatically and apply pending migrations.

## Open Questions

- Which IP geolocation service should be used (e.g., a free HTTP API, Cloudflare headers, or a self-hosted service)?
- Should the language selector be placed in the top navigation bar in addition to Settings for quicker access?
