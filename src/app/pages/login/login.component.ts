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
  protected mode = signal<'login' | 'register'>('login');
  protected email = signal('');
  protected password = signal('');
  protected nome = signal('');
  protected nomeEmpresa = signal('');
  protected urlSlug = signal('');
  private slugEdited = false;
  protected loading = signal(false);
  protected errorMessage = signal('');

  constructor(
    private readonly authService: AuthService,
    private readonly router: Router
  ) {}

  protected toggleMode(): void {
    this.mode.set(this.mode() === 'login' ? 'register' : 'login');
    this.errorMessage.set('');
  }

  protected onNomeEmpresaChange(value: string): void {
    this.nomeEmpresa.set(value);
    if (!this.slugEdited) {
      this.urlSlug.set(this.slugify(value));
    }
  }

  protected onSlugChange(value: string): void {
    this.slugEdited = value.length > 0;
    this.urlSlug.set(this.slugify(value));
  }

  private slugify(value: string): string {
    return value
      .toLowerCase()
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .replace(/[^a-z0-9]+/g, '-')
      .replace(/^-+|-+$/g, '');
  }

  protected async onSubmit(): Promise<void> {
    this.loading.set(true);
    this.errorMessage.set('');

    if (this.mode() === 'login') {
      const success = await this.authService.login(this.email(), this.password());
      if (success) {
        this.router.navigate(['/dashboard']);
      } else {
        this.errorMessage.set('Email ou senha invalidos');
      }
    } else {
      if (!this.nome() || !this.email() || !this.password() || !this.nomeEmpresa() || !this.urlSlug()) {
        this.errorMessage.set('Preencha todos os campos');
        this.loading.set(false);
        return;
      }
      const result = await this.authService.register({
        nome: this.nome(),
        email: this.email(),
        senha: this.password(),
        nomeEmpresa: this.nomeEmpresa(),
        urlSlug: this.urlSlug()
      });
      if (result.success) {
        this.router.navigate(['/dashboard']);
      } else {
        this.errorMessage.set(result.message ?? 'Nao foi possivel concluir o cadastro');
      }
    }

    this.loading.set(false);
  }
}
