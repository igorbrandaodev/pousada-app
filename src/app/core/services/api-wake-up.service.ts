import { Injectable, signal, computed } from '@angular/core';

export type WakeUpStatus = 'idle' | 'waking' | 'success';

/**
 * Coordinates the "server is waking up" UX for the free-tier Azure API/DB that
 * auto-pauses after inactivity. The interceptor reports slow/cold requests here;
 * a global overlay reacts to the exposed signals.
 */
@Injectable({ providedIn: 'root' })
export class ApiWakeUpService {
  // Rough time (seconds) a cold start typically takes — drives the progress bar.
  private readonly ESTIMATE_SECONDS = 45;

  private activeCount = 0;
  private timerId: ReturnType<typeof setInterval> | null = null;
  private resetTimeoutId: ReturnType<typeof setTimeout> | null = null;

  public readonly status = signal<WakeUpStatus>('idle');
  public readonly elapsedSeconds = signal(0);

  public readonly visible = computed(() => this.status() !== 'idle');

  // Eases toward 92% while waking; snaps to 100% on success.
  public readonly progress = computed(() => {
    const s = this.status();
    if (s === 'success') return 100;
    if (s === 'idle') return 0;
    const pct = (this.elapsedSeconds() / this.ESTIMATE_SECONDS) * 92;
    return Math.min(92, Math.round(pct));
  });

  /** A request has detected the server is slow/cold and is now waiting/retrying. */
  public beginWaking(): void {
    this.activeCount++;
    if (this.status() !== 'waking') {
      this.clearResetTimeout();
      this.status.set('waking');
      this.elapsedSeconds.set(0);
      this.startTimer();
    }
  }

  /**
   * A request that was waking has finished. On success we flash a brief "ready"
   * state; on a non-retryable error we simply hide (the caller handles the error).
   */
  public endWaking(success: boolean): void {
    this.activeCount = Math.max(0, this.activeCount - 1);
    if (this.activeCount > 0 || this.status() !== 'waking') return;

    this.stopTimer();
    this.clearResetTimeout();

    if (!success) {
      this.status.set('idle');
      this.elapsedSeconds.set(0);
      return;
    }

    this.status.set('success');
    this.resetTimeoutId = setTimeout(() => {
      if (this.status() === 'success') {
        this.status.set('idle');
        this.elapsedSeconds.set(0);
      }
    }, 1400);
  }

  private startTimer(): void {
    this.stopTimer();
    this.timerId = setInterval(() => this.elapsedSeconds.update(v => v + 1), 1000);
  }

  private stopTimer(): void {
    if (this.timerId) {
      clearInterval(this.timerId);
      this.timerId = null;
    }
  }

  private clearResetTimeout(): void {
    if (this.resetTimeoutId) {
      clearTimeout(this.resetTimeoutId);
      this.resetTimeoutId = null;
    }
  }
}
