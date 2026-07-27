import { Component, OnInit, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SettingsService } from '../../services/settings.service';
import { UserProfile } from '../../models/user.model';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './settings.html'
})
export class Settings implements OnInit, OnDestroy {
  profile: UserProfile | null = null;
  theme = 'auto';
  success = '';
  error = '';
  loading = true;
  loadError = '';
  private mediaQuery?: MediaQueryList;
  private mediaHandler?: (e: MediaQueryListEvent) => void;

  constructor(
    private settingsService: SettingsService,
    public authService: AuthService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadProfile();
  }

  ngOnDestroy(): void {
    this.mediaQuery?.removeEventListener('change', this.mediaHandler!);
  }

  loadProfile(): void {
    this.loading = true;
    this.loadError = '';
    this.settingsService.getProfile().subscribe({
      next: (profile) => {
        this.profile = profile;
        if (profile.settings?.theme) {
          this.theme = profile.settings.theme;
        }
        this.applyTheme(this.theme);
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.loadError = err.error?.message || 'Failed to load profile';
        this.loading = false;
        this.cdr.markForCheck();
      }
    });
  }

  updateSettings(): void {
    this.success = '';
    this.error = '';
    this.settingsService.updateSettings({
      theme: this.theme
    }).subscribe({
      next: () => {
        this.success = 'Settings updated';
        this.applyTheme(this.theme);
        this.cdr.markForCheck();
      },
      error: (err) => { this.error = err.error?.message || 'Update failed'; this.cdr.markForCheck(); }
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
