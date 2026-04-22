import { Injectable, signal, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment';

interface User {
  nome: string;
  email: string;
  role: string;
  empresaId: number;
}

interface LoginResponse {
  token: string;
  nome: string;
  email: string;
  role: string;
  empresaId: number;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  public isLoggedIn = signal(false);
  public currentUser = signal<User | null>(null);

  constructor() {
    const token = localStorage.getItem('pousada_token');
    const storedUser = localStorage.getItem('pousada_user');
    if (token && storedUser) {
      this.isLoggedIn.set(true);
      this.currentUser.set(JSON.parse(storedUser));
    }
  }

  login(email: string, senha: string): Promise<boolean> {
    return new Promise((resolve) => {
      this.http.post<LoginResponse>(`${environment.apiUrl}/Auth/login`, { email, senha })
        .subscribe({
          next: (res) => {
            localStorage.setItem('pousada_token', res.token);
            const user: User = { nome: res.nome, email: res.email, role: res.role, empresaId: res.empresaId };
            localStorage.setItem('pousada_user', JSON.stringify(user));
            this.currentUser.set(user);
            this.isLoggedIn.set(true);
            resolve(true);
          },
          error: () => resolve(false)
        });
    });
  }

  logout(): void {
    localStorage.removeItem('pousada_token');
    localStorage.removeItem('pousada_user');
    this.currentUser.set(null);
    this.isLoggedIn.set(false);
    this.router.navigate(['/login']);
  }
}
