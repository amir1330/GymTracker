import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

export const SUPPORTED_LANGUAGES = ['kz', 'ru', 'en'] as const;
export type SupportedLanguage = (typeof SUPPORTED_LANGUAGES)[number];

interface IpApiResponse {
  country_code?: string;
  country?: string;
}

@Injectable({
  providedIn: 'root'
})
export class LocaleDetectionService {
  constructor(private http: HttpClient) {}

  detectFromBrowser(): SupportedLanguage | null {
    const raw = navigator.language || (navigator as any).userLanguage || 'en';
    return this.mapToSupported(raw);
  }

  async detectFromIp(): Promise<SupportedLanguage> {
    try {
      const data = await firstValueFrom(
        this.http.get<IpApiResponse>('https://ipapi.co/json/', {
          headers: { Accept: 'application/json' }
        })
      );
      const country = (data.country_code || data.country || '').toUpperCase();
      if (country === 'KZ' || country === 'KAZAKHSTAN') return 'kz';
      if (country === 'RU' || country === 'RUSSIA' || country === 'RUSSIAN FEDERATION') return 'ru';
      return 'en';
    } catch {
      return 'en';
    }
  }

  mapToSupported(locale: string): SupportedLanguage | null {
    const lang = locale.toLowerCase().split('-')[0];
    if (lang === 'kk' || lang === 'kz') return 'kz';
    if (lang === 'ru') return 'ru';
    if (lang === 'en') return 'en';
    return null;
  }

  async detectDefault(): Promise<SupportedLanguage> {
    const browser = this.detectFromBrowser();
    if (browser) return browser;
    return this.detectFromIp();
  }
}
