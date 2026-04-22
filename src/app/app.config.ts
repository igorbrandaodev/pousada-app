import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { providePrimeNG } from 'primeng/config';
import { definePreset } from '@primeng/themes';
import Aura from '@primeng/themes/aura';

import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';

const CanastraPreset = definePreset(Aura, {
  semantic: {
    primary: {
      50: '#FDF2EC', 100: '#FADED0', 200: '#E8B999', 300: '#D4945F', 400: '#C87A42',
      500: '#B7622B', 600: '#A05524', 700: '#8B4A1D', 800: '#6E3A17', 900: '#522C12', 950: '#3A1E0C'
    },
    colorScheme: {
      light: {
        surface: {
          0: '#FFFFFF', 50: '#FAFAF8', 100: '#F5F3F0', 200: '#E8E3DD',
          300: '#E0DBD5', 400: '#C4BEB6', 500: '#8C857E', 600: '#6B6560',
          700: '#4A4540', 800: '#2D2A26', 900: '#1A1816', 950: '#0D0C0B'
        }
      }
    }
  }
});

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideAnimationsAsync(),
    provideHttpClient(withInterceptors([authInterceptor])),
    providePrimeNG({
      theme: {
        preset: CanastraPreset,
        options: {
          darkModeSelector: '.dark-mode'
        }
      }
    })
  ]
};
