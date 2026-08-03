import { Component, ChangeDetectorRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { AuthService } from '../../services/auth.service';
import { TranslationService } from '../../services/translation.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [FormsModule, RouterModule, TranslatePipe],
  templateUrl: './register.html'
})
export class Register {
  email = '';
  password = '';
  confirmPassword = '';
  error = '';

  constructor(
    private authService: AuthService,
    private router: Router,
    private cdr: ChangeDetectorRef,
    private translationService: TranslationService
  ) {}

  onSubmit(): void {
    this.error = '';
    const email = this.email.trim();
    if (!email || !this.password) {
      this.error = this.translationService.instant('auth.emailRequired');
      return;
    }
    if (this.password !== this.confirmPassword) {
      this.error = this.translationService.instant('auth.passwordsMismatch');
      return;
    }
    this.authService.register({
      email,
      password: this.password,
      confirmPassword: this.confirmPassword,
      language: this.translationService.getCurrentLanguage()
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
          this.error = this.translationService.instant('auth.registrationFailed');
        }
        this.cdr.markForCheck();
      }
    });
  }
}
