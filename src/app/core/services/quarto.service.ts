import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Quarto } from '../models';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class QuartoService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiUrl}/Quartos`;

  private _quartos = signal<Quarto[]>([]);
  public quartos = this._quartos.asReadonly();

  public quartosDisponiveis = computed(() =>
    this._quartos().filter(q => q.status === 'disponivel')
  );

  public quartosOcupados = computed(() =>
    this._quartos().filter(q => q.status === 'ocupado')
  );

  constructor() {
    this.loadAll();
  }

  loadAll(): void {
    this.http.get<any[]>(this.url).subscribe({
      next: (data) => this._quartos.set(data.map(q => this.mapQuarto(q))),
      error: (err) => console.error('Failed to load quartos:', err)
    });
  }

  getById(id: number): Quarto | undefined {
    return this._quartos().find(q => q.id === id);
  }

  add(quarto: Partial<Quarto>): void {
    this.http.post(this.url, {
      numero: quarto.numero,
      tipo: this.capitalize(quarto.tipo || 'standard'),
      capacidade: quarto.capacidade,
      precoDiaria: quarto.precoDiaria,
      andar: quarto.andar
    }).subscribe({ next: () => this.loadAll() });
  }

  update(quarto: Quarto): void {
    this.http.put(`${this.url}/${quarto.id}`, {
      numero: quarto.numero,
      tipo: this.capitalize(quarto.tipo),
      capacidade: quarto.capacidade,
      precoDiaria: quarto.precoDiaria,
      status: this.capitalize(quarto.status),
      andar: quarto.andar,
      ativo: true
    }).subscribe({ next: () => this.loadAll() });
  }

  updateStatus(id: number, status: Quarto['status']): void {
    this.http.patch(`${this.url}/${id}/status`, { status: this.capitalize(status) })
      .subscribe({ next: () => this.loadAll() });
  }

  private mapQuarto(q: any): Quarto {
    return {
      ...q,
      tipo: q.tipo?.toLowerCase() || 'standard',
      status: q.status?.toLowerCase() || 'disponivel',
      amenidades: q.amenidades || []
    };
  }

  private capitalize(s: string): string {
    return s.charAt(0).toUpperCase() + s.slice(1).toLowerCase();
  }
}
