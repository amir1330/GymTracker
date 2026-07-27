import { Component, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './login.html'
})
export class Login {
  email = '';
  password = '';
  error = '';

  constructor(private authService: AuthService, private router: Router, private cdr: ChangeDetectorRef) {}

  onSubmit(): void {
    this.error = '';
    const email = this.email.trim();
    if (!email || !this.password) {
      this.error = 'Email and password are required';
      return;
    }
    this.authService.login({ email, password: this.password }).subscribe({
      next: () => this.router.navigate(['/workouts']),
      error: (err) => {
        const body = err.error;
        if (typeof body === 'string') {
          this.error = body;
        } else if (body?.message) {
          this.error = body.message;
        } else {
          this.error = 'Login failed';
        }
        this.cdr.markForCheck();
      }
    });
  }
}
