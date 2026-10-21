import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ApiWakeUpOverlayComponent } from './shared/api-wake-up-overlay/api-wake-up-overlay.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, ApiWakeUpOverlayComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  protected readonly title = 'pousada-app';
}
