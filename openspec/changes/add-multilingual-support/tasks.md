## 1. Backend — Extend UserSettings

- [x] 1.1 Add `Language` property to `backend/Models/UserSettings.cs` with allowed values `kz`, `ru`, `en` and default `en`.
- [x] 1.2 Add `Language` property to `backend/DTOs/User/UpdateSettingsRequest.cs` with validation for supported values.
- [x] 1.3 Update `backend/Services/UserService.cs` to save and return the `Language` value.
- [x] 1.4 Update `backend/Mappings/UserProfile.cs` to map `Language` between DTOs and entity.
- [x] 1.5 Create and apply EF Core migration for the new `Language` column with existing rows defaulting to `en`.
- [x] 1.6 Update registration flow to set the initial language from detected locale/IP geolocation.

## 2. Frontend — Add i18n Infrastructure

- [x] 2.1 Install `@ngx-translate/core` and `@ngx-translate/http-loader` in `frontend/`.
- [x] 2.2 Create translation files `frontend/public/assets/i18n/kz.json`, `ru.json`, and `en.json` with a shared key namespace.
- [x] 2.3 Add `TranslateModule`/`provideTranslation` configuration in `frontend/src/app/app.config.ts` with default/fallback language `en`.
- [x] 2.4 Create `frontend/src/app/services/translation.service.ts` to wrap `TranslateService` and expose helper methods for programmatic translation and language switching.

## 3. Frontend — Locale Detection

- [x] 3.1 Create `frontend/src/app/services/locale-detection.service.ts` that reads `navigator.language`, maps supported locales to `kz`/`ru`/`en`, and falls back to IP geolocation.
- [x] 3.2 Implement IP geolocation fallback using a lightweight HTTP service (e.g., `https://ipapi.co/json/` or Cloudflare headers) to detect Kazakhstan/Russia/Other and map to `kz`/`ru`/`en`.
- [x] 3.3 On app bootstrap, resolve the initial language from saved preference first, then locale detection, and call `translate.use(language)`.

## 4. Frontend — Language Switcher in Settings

- [x] 4.1 Add a language selector control to `frontend/src/app/pages/settings/settings.html` with options `Қазақша`/`kz`, `Русский`/`ru`, `English`/`en`.
- [x] 4.2 Wire the selector to `TranslationService.setLanguage()` and apply the change immediately.
- [x] 4.3 For authenticated users, persist the selected language by calling the backend settings endpoint.
- [x] 4.4 For guest users, store the selected language in `localStorage`.

## 5. Frontend — Translate UI Strings

- [x] 5.1 Replace hard-coded strings in `app.html` navigation with translation keys.
- [x] 5.2 Replace hard-coded strings in `pages/login/` with translation keys.
- [x] 5.3 Replace hard-coded strings in `pages/register/` with translation keys.
- [x] 5.4 Replace hard-coded strings in `pages/workout-list/` and `pages/workout-form/` with translation keys.
- [x] 5.5 Replace hard-coded strings in `pages/preset-list/` and `pages/preset-form/` with translation keys.
- [x] 5.6 Replace hard-coded strings in `pages/exercise-list/` and `pages/exercise-form/` with translation keys.
- [x] 5.7 Replace hard-coded strings in `pages/progress/` and chart components with translation keys.
- [x] 5.8 Replace hard-coded strings in `pages/settings/` with translation keys.
- [x] 5.9 Replace hard-coded strings in shared components (`chart-tile`, `chart-editor`, `onboarding-guide`) with translation keys.
- [x] 5.10 Translate programmatic strings generated in services/components (e.g., confirmation messages, dynamic labels).

## 6. Translation Content

- [x] 6.1 Populate `en.json` with all English translation keys used in the app.
- [x] 6.2 Populate `ru.json` with Russian translations for all keys.
- [x] 6.3 Populate `kz.json` with Kazakh translations for all keys.

## 7. Verification & Deployment

- [x] 7.1 Run `dotnet build` and `dotnet test` (if tests exist) in the `backend/` directory.
- [x] 7.2 Run `ng build` in the `frontend/` directory to confirm no translation-related build errors.
- [x] 7.3 Manually verify language switching in Settings updates all pages without reload.
- [x] 7.4 Verify default language detection works for browser locales `kk`, `ru`, `en`, and unsupported locales.
- [x] 7.5 Verify the backend migration applies cleanly on a fresh/seeded database.
- [x] 7.6 Commit the changes and push to the remote repository to trigger the existing CI/CD deployment.
