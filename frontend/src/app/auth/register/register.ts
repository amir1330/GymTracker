import { Component, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './register.html'
})
export class Register {
  username = '';
  email = '';
  password = '';
  weight?: number;
  height?: number;
  error = '';

  constructor(private authService: AuthService, private router: Router, private cdr: ChangeDetectorRef) {}

  onSubmit(): void {
    this.error = '';
    const username = this.username.trim();
    const email = this.email.trim();
    if (!username || !email || !this.password) {
      this.error = 'Username, email and password are required';
      return;
    }
    this.authService.register({
      username,
      email,
      password: this.password,
      weight: this.weight,
      height: this.height
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
