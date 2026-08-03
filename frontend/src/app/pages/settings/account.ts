import { Component, OnInit, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { SettingsService } from '../../services/settings.service';
import { AuthService } from '../../services/auth.service';
import { TranslationService } from '../../services/translation.service';
import { SupportedLanguage } from '../../services/locale-detection.service';

@Component({
  selector: 'app-account',
  standalone: true,
  imports: [FormsModule, TranslatePipe],
  templateUrl: './account.html'
})
export class Account implements OnInit, OnDestroy {
  email = '';
  theme = 'auto';
  language: SupportedLanguage = 'en';
  success = '';
  private mediaQuery?: MediaQueryList;
  private mediaHandler?: (e: MediaQueryListEvent) => void;

  constructor(
    private settingsService: SettingsService,
    public authService: AuthService,
    private cdr: ChangeDetectorRef,
    private translationService: TranslationService
  ) {}

  ngOnInit(): void {
    this.loadEmailFromToken();
    this.loadTheme();
    this.language = this.translationService.getCurrentLanguage();
  }

  ngOnDestroy(): void {
    this.mediaQuery?.removeEventListener('change', this.mediaHandler!);
  }

  private loadEmailFromToken(): void {
    const token = localStorage.getItem('token');
    if (token) {
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        this.email = payload.email || '';
      } catch {}
    }
    this.cdr.markForCheck();
  }

  private loadTheme(): void {
    this.theme = localStorage.getItem('theme') || 'auto';
    this.applyTheme(this.theme);
    this.cdr.markForCheck();
  }

  updateSettings(): void {
    this.success = '';
    this.settingsService.updateSettings({
      theme: this.theme,
      language: this.language
    }).subscribe(() => {
      this.success = this.translationService.instant('settings.updated');
      this.applyTheme(this.theme);
      this.cdr.markForCheck();
    });
  }

  setLanguage(language: SupportedLanguage): void {
    this.language = language;
    this.translationService.setLanguage(language);
    this.success = this.translationService.instant('settings.updated');
    this.cdr.markForCheck();
  }

  applyTheme(theme: string): void {
    this.mediaQuery?.removeEventListener('change', this.mediaHandler!);

    if (theme === 'auto') {
      this.mediaQuery = window.matchMedia('(prefers-color-scheme: light)');
      this.mediaHandler = () => this.applyOsTheme();
      this.mediaQuery.addEventListener('change', this.mediaHandler);
      this.applyOsTheme();
    } else if (theme === 'light') {
      document.documentElement.setAttribute('data-theme', 'light');
    } else {
      document.documentElement.removeAttribute('data-theme');
    }
    localStorage.setItem('theme', theme);
  }

  private applyOsTheme(): void {
    const isLight = window.matchMedia('(prefers-color-scheme: light)').matches;
    if (isLight) {
      document.documentElement.setAttribute('data-theme', 'light');
    } else {
      document.documentElement.removeAttribute('data-theme');
    }
  }

  setTheme(t: string): void {
    this.theme = t;
    this.applyTheme(t);
    this.updateSettings();
  }
}
