import { inject } from '@angular/core';
import { HttpInterceptorFn, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, timer, TimeoutError } from 'rxjs';
import { retry, timeout } from 'rxjs/operators';
import { ApiWakeUpService } from '../services/api-wake-up.service';
import { environment } from '../../environments/environment';

// Show the overlay if a request to our API hasn't answered in this long.
const SLOW_MS = 4500;
// Each attempt waits this long for a (possibly DB-resuming) hang before retrying.
const ATTEMPT_TIMEOUT_MS = 50000;
// Delay between retries while the server/DB is waking up.
const RETRY_DELAY_MS = 7000;

/**
 * Retry any transient or server-side failure — the free Azure SQL DB auto-pauses
 * and surfaces as timeouts, 5xx (incl. 500 when the DB query can't connect), or
 * connection errors while it resumes. We keep retrying until it comes back.
 * Genuine client errors (400/401/403/404/409/422 …) are NOT retried so real
 * problems (e.g. wrong password) surface immediately.
 */
function isRetryable(error: unknown): boolean {
  if (error instanceof TimeoutError) return true;
  if (error instanceof HttpErrorResponse) {
    const s = error.status;
    return s === 0 || s === 408 || s === 429 || s >= 500;
  }
  return false;
}

export const apiWakeUpInterceptor: HttpInterceptorFn = (req, next) => {
  // Only our own API can "wake up"; leave any third-party calls alone.
  if (!req.url.startsWith(environment.apiUrl)) {
    return next(req);
  }

  const wakeUp = inject(ApiWakeUpService);

  return new Observable<HttpEvent<unknown>>(subscriber => {
    let waking = false;
    let settled = false;

    const markWaking = () => {
      if (!waking) {
        waking = true;
        wakeUp.beginWaking();
      }
    };

    // A merely-slow (but eventually successful) request still shows the overlay.
    const slowTimer = setTimeout(() => {
      if (!settled) markWaking();
    }, SLOW_MS);

    const source = next(req).pipe(
      timeout({ each: ATTEMPT_TIMEOUT_MS }),
      retry({
        // Effectively unlimited: keep retrying until the server wakes up.
        delay: (error) => {
          if (isRetryable(error)) {
            markWaking();
            return timer(RETRY_DELAY_MS);
          }
          return throwError(() => error);
        }
      })
    );

    const sub = source.subscribe({
      next: value => subscriber.next(value),
      error: error => {
        settled = true;
        clearTimeout(slowTimer);
        if (waking) wakeUp.endWaking(false);
        // Unwrap a bare TimeoutError into something the app's error handlers expect.
        subscriber.error(error instanceof TimeoutError
          ? new HttpErrorResponse({ status: 504, statusText: 'Gateway Timeout', url: req.url })
          : error);
      },
      complete: () => {
        settled = true;
        clearTimeout(slowTimer);
        if (waking) wakeUp.endWaking(true);
        subscriber.complete();
      }
    });

    return () => {
      clearTimeout(slowTimer);
      // If the request is cancelled (e.g. navigation destroys the caller) after the
      // overlay was shown, still balance the counter so it never gets stuck.
      if (!settled) {
        settled = true;
        if (waking) wakeUp.endWaking(false);
      }
      sub.unsubscribe();
    };
  });
};
