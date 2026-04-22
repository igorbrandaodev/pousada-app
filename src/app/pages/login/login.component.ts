import { Component, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { MessageModule } from 'primeng/message';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, ButtonModule, InputTextModule, PasswordModule, MessageModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  protected email = signal('');
  protected password = signal('');
  protected loading = signal(false);
  protected errorMessage = signal('');

  constructor(
    private readonly authService: AuthService,
    private readonly router: Router
  ) {}

  protected async onSubmit(): Promise<void> {
    this.loading.set(true);
    this.errorMessage.set('');
    const success = await this.authService.login(this.email(), this.password());
    if (success) {
      this.router.navigate(['/dashboard']);
    } else {
      this.errorMessage.set('Email ou senha invalidos');
    }
    this.loading.set(false);
  }
}
