import { Component, OnInit, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { SettingsService } from '../../services/settings.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-account',
  standalone: true,
  imports: [],
  templateUrl: './account.html'
})
export class Account implements OnInit, OnDestroy {
  email = '';
  theme = 'auto';
  success = '';
  private mediaQuery?: MediaQueryList;
  private mediaHandler?: (e: MediaQueryListEvent) => void;

  constructor(
    private settingsService: SettingsService,
    public authService: AuthService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadEmailFromToken();
    this.loadTheme();
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
      theme: this.theme
    }).subscribe(() => {
      this.success = 'Settings updated';
      this.applyTheme(this.theme);
      this.cdr.markForCheck();
    });
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
