import { Injectable } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { firstValueFrom } from 'rxjs';
import {
  LocaleDetectionService,
  SupportedLanguage,
  SUPPORTED_LANGUAGES
} from './locale-detection.service';
import { SettingsService } from './settings.service';
import { AuthService } from './auth.service';

export const LANGUAGE_STORAGE_KEY = 'app-language';

@Injectable({
  providedIn: 'root'
})
export class TranslationService {
  constructor(
    private translate: TranslateService,
    private localeDetection: LocaleDetectionService,
    private settingsService: SettingsService,
    private authService: AuthService
  ) {}

  async initialize(): Promise<void> {
    this.translate.addLangs(SUPPORTED_LANGUAGES as unknown as string[]);

    const savedLanguage = this.getStoredLanguage();
    if (savedLanguage) {
      await this.useLanguage(savedLanguage);
      return;
    }

    if (this.authService.isLoggedIn()) {
      try {
        const settings = await firstValueFrom(this.settingsService.getSettings());
        if (settings?.language && this.isSupported(settings.language)) {
          await this.useLanguage(settings.language);
          this.storeLanguage(settings.language);
          return;
        }
      } catch {
        // ignore and fall back to detection
      }
    }

    const detected = await this.localeDetection.detectDefault();
    await this.useLanguage(detected);
    this.storeLanguage(detected);
  }

  async useLanguage(language: SupportedLanguage): Promise<void> {
    if (!this.isSupported(language)) return;
    await firstValueFrom(this.translate.use(language));
  }

  setLanguage(language: SupportedLanguage): void {
    this.useLanguage(language);
    this.storeLanguage(language);

    if (this.authService.isLoggedIn()) {
      this.settingsService.updateSettings({ language }).subscribe();
    }
  }

  getCurrentLanguage(): SupportedLanguage {
    const lang = this.translate.getCurrentLang() || this.translate.getFallbackLang() || 'en';
    return this.isSupported(lang) ? lang : 'en';
  }

  instant(key: string | string[], interpolateParams?: object): string {
    return this.translate.instant(key, interpolateParams);
  }

  private isSupported(value: string): value is SupportedLanguage {
    return (SUPPORTED_LANGUAGES as readonly string[]).includes(value);
  }

  private getStoredLanguage(): SupportedLanguage | null {
    const value = localStorage.getItem(LANGUAGE_STORAGE_KEY);
    if (value && this.isSupported(value)) return value;
    return null;
  }

  private storeLanguage(language: SupportedLanguage): void {
    localStorage.setItem(LANGUAGE_STORAGE_KEY, language);
  }
}
