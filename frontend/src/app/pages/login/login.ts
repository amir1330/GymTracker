import { Component, ChangeDetectorRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { AuthService } from '../../services/auth.service';
import { TranslationService } from '../../services/translation.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, RouterModule, TranslatePipe],
  templateUrl: './login.html'
})
export class Login {
  email = '';
  password = '';
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
    this.authService.login({ email, password: this.password }).subscribe({
      next: () => this.router.navigate(['/workouts']),
      error: (err) => {
        const body = err.error;
        if (typeof body === 'string') {
          this.error = body;
        } else if (body?.message) {
          this.error = body.message;
        } else {
          this.error = this.translationService.instant('auth.loginFailed');
        }
        this.cdr.markForCheck();
      }
    });
  }
}
