import { HttpErrorResponse } from '@angular/common/http';
import { TranslationService } from '../services/translation.service';

export function getAuthErrorMessage(
  err: HttpErrorResponse,
  translationService: TranslationService,
  fallbackKey: 'auth.loginFailed' | 'auth.registrationFailed'
): string {
  if (err.status === 0) {
    return translationService.instant('auth.serverUnreachable');
  }

  const body = err.error;
  if (typeof body === 'string' && body.trim()) {
    return body;
  }
  if (body?.message) {
    return body.message;
  }
  if (Array.isArray(body)) {
    return body.map((e: { description?: string }) => e.description).filter(Boolean).join('. ');
  }

  return translationService.instant(fallbackKey);
}
