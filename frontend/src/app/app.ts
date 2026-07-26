import { Component, OnInit } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from './auth/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, CommonModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  constructor(public authService: AuthService) {}

  ngOnInit(): void {
    const savedTheme = localStorage.getItem('theme') || 'dark';
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
