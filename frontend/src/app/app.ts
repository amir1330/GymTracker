import { Component, OnInit } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { AuthService } from './services/auth.service';
import { OnboardingGuide } from './components/onboarding-guide/onboarding-guide';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, OnboardingGuide, TranslatePipe],
  templateUrl: './app.html'
})
export class App implements OnInit {
  constructor(public authService: AuthService) {}

  ngOnInit(): void {
    const savedTheme = localStorage.getItem('theme') || 'auto';
    if (savedTheme === 'auto') {
      const isLight = window.matchMedia('(prefers-color-scheme: light)').matches;
      if (isLight) {
        document.documentElement.setAttribute('data-theme', 'light');
      }
    } else if (savedTheme === 'light') {
      document.documentElement.setAttribute('data-theme', 'light');
    }
  }
}
