import { Component, inject, ChangeDetectionStrategy } from '@angular/core';
import { ApiWakeUpService } from '../../core/services/api-wake-up.service';

@Component({
  selector: 'app-api-wake-up-overlay',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (svc.visible()) {
    <div class="wakeup-backdrop">
      <div class="wakeup-card">
        @if (svc.status() === 'waking') {
        <div class="wakeup-icon spinning">
          <i class="pi pi-bolt"></i>
        </div>
        <h2 class="wakeup-title">Ligando os motores…</h2>
        <p class="wakeup-text">
          O servidor entrou em modo de espera por inatividade (plano gratuito).
          Estamos acordando ele — isso costuma levar de 30 a 60 segundos. Sua
          acao continuara <strong>automaticamente</strong> assim que estiver pronto.
        </p>
        <div class="wakeup-progress">
          <div class="wakeup-progress-bar" [style.width.%]="svc.progress()"></div>
        </div>
        <div class="wakeup-timer">
          <i class="pi pi-clock"></i> {{ svc.elapsedSeconds() }}s decorridos
        </div>
        } @else if (svc.status() === 'success') {
        <div class="wakeup-icon success">
          <i class="pi pi-check-circle"></i>
        </div>
        <h2 class="wakeup-title">Pronto!</h2>
        <p class="wakeup-text">Servidor no ar — continuando…</p>
        }
      </div>
    </div>
    }
  `,
  styles: [`
    .wakeup-backdrop {
      position: fixed;
      inset: 0;
      z-index: 100000;
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 20px;
      background: rgba(15, 23, 42, 0.55);
      backdrop-filter: blur(4px);
      animation: wakeup-fade 0.2s ease;
    }

    .wakeup-card {
      width: 100%;
      max-width: 420px;
      background: var(--surface-card, #ffffff);
      border-radius: 16px;
      padding: 32px 28px;
      text-align: center;
      box-shadow: 0 20px 50px rgba(0, 0, 0, 0.3);
    }

    .wakeup-icon {
      width: 64px;
      height: 64px;
      margin: 0 auto 18px;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 1.9rem;
      color: #fff;
    }

    .wakeup-icon.spinning {
      background: linear-gradient(135deg, #f59e0b 0%, #f97316 100%);
      animation: wakeup-pulse 1.2s ease-in-out infinite;
    }
    .wakeup-icon.spinning i {
      animation: wakeup-buzz 0.9s ease-in-out infinite;
    }
    .wakeup-icon.success {
      background: linear-gradient(135deg, #16a34a 0%, #22c55e 100%);
    }

    .wakeup-title {
      font-size: 1.25rem;
      font-weight: 700;
      color: var(--text, #1e293b);
      margin: 0 0 10px;
    }

    .wakeup-text {
      font-size: 0.9rem;
      line-height: 1.5;
      color: var(--text-secondary, #475569);
      margin: 0 0 20px;
    }

    .wakeup-progress {
      width: 100%;
      height: 8px;
      background: var(--border, #e2e8f0);
      border-radius: 999px;
      overflow: hidden;
      margin-bottom: 12px;
    }

    .wakeup-progress-bar {
      height: 100%;
      border-radius: 999px;
      background: linear-gradient(90deg, #f59e0b 0%, #f97316 100%);
      transition: width 0.6s ease;
      background-size: 200% 100%;
      animation: wakeup-shimmer 1.6s linear infinite;
    }

    .wakeup-timer {
      font-size: 0.8rem;
      color: var(--text-secondary, #64748b);
      font-weight: 600;
    }

    @keyframes wakeup-fade {
      from { opacity: 0; }
      to { opacity: 1; }
    }
    @keyframes wakeup-pulse {
      0%, 100% { transform: scale(1); box-shadow: 0 0 0 0 rgba(249, 115, 22, 0.4); }
      50% { transform: scale(1.06); box-shadow: 0 0 0 12px rgba(249, 115, 22, 0); }
    }
    @keyframes wakeup-buzz {
      0%, 100% { transform: rotate(-8deg); }
      50% { transform: rotate(8deg); }
    }
    @keyframes wakeup-shimmer {
      0% { background-position: 200% 0; }
      100% { background-position: -200% 0; }
    }
  `]
})
export class ApiWakeUpOverlayComponent {
  protected svc = inject(ApiWakeUpService);
}
