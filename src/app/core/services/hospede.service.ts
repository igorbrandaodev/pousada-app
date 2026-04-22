import { Injectable, signal, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Hospede } from '../models';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class HospedeService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiUrl}/Hospedes`;

  private _hospedes = signal<Hospede[]>([]);
  public hospedes = this._hospedes.asReadonly();

  constructor() {
    this.loadAll();
  }

  loadAll(): void {
    this.http.get<Hospede[]>(this.url).subscribe({
      next: (data) => this._hospedes.set(data),
      error: (err) => console.error('Failed to load hospedes:', err)
    });
  }

  getById(id: number): Hospede | undefined {
    return this._hospedes().find(h => h.id === id);
  }

  add(hospede: Partial<Hospede>): void {
    this.http.post(this.url, hospede).subscribe({ next: () => this.loadAll() });
  }

  update(hospede: Hospede): void {
    this.http.put(`${this.url}/${hospede.id}`, {
      nome: hospede.nome,
      documento: hospede.documento,
      telefone: hospede.telefone,
      email: hospede.email,
      cidade: hospede.cidade,
      observacoes: hospede.observacoes
    }).subscribe({ next: () => this.loadAll() });
  }

  search(term: string): Hospede[] {
    const lower = term.toLowerCase();
    return this._hospedes().filter(h =>
      h.nome.toLowerCase().includes(lower) ||
      h.documento.includes(term) ||
      h.email.toLowerCase().includes(lower)
    );
  }
}
