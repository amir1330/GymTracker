import { Component, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './register.html'
})
export class Register {
  email = '';
  password = '';
  confirmPassword = '';
  weight?: number;
  error = '';

  constructor(private authService: AuthService, private router: Router, private cdr: ChangeDetectorRef) {}

  onSubmit(): void {
    this.error = '';
    const email = this.email.trim();
    if (!email || !this.password) {
      this.error = 'Email and password are required';
      return;
    }
    if (this.password !== this.confirmPassword) {
      this.error = 'Passwords do not match';
      return;
    }
    this.authService.register({
      email,
      password: this.password,
      confirmPassword: this.confirmPassword,
      weight: this.weight
    }).subscribe({
      next: () => this.router.navigate(['/workouts']),
      error: (err) => {
        const body = err.error;
        if (typeof body === 'string') {
          this.error = body;
        } else if (body?.message) {
          this.error = body.message;
        } else if (Array.isArray(body)) {
          this.error = body.map((e: any) => e.description).join('. ');
        } else {
          this.error = 'Registration failed';
        }
        this.cdr.markForCheck();
      }
    });
  }
}
