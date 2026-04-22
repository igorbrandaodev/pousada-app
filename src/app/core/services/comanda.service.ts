import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Comanda, ItemComanda } from '../models';
import { ProdutoService } from './produto.service';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ComandaService {
  private readonly http = inject(HttpClient);
  private readonly produtoService = inject(ProdutoService);
  private readonly url = `${environment.apiUrl}/Comandas`;

  private _comandas = signal<Comanda[]>([]);
  public comandas = this._comandas.asReadonly();

  public comandasAbertas = computed(() =>
    this._comandas()
      .filter(c => c.status === 'aberta')
      .map(c => this.populateComanda(c))
  );

  constructor() {
    this.loadAll();
  }

  loadAll(): void {
    this.http.get<any[]>(this.url).subscribe({
      next: (data) => this._comandas.set(data.map(c => this.mapComanda(c))),
      error: (err) => console.error('Failed to load comandas:', err)
    });
  }

  getById(id: number): Comanda | undefined {
    const comanda = this._comandas().find(c => c.id === id);
    return comanda ? this.populateComanda(comanda) : undefined;
  }

  getByReserva(idReserva: number): Comanda[] {
    return this._comandas()
      .filter(c => c.idReserva === idReserva)
      .map(c => this.populateComanda(c));
  }

  abrirComanda(idReserva: number): void {
    this.http.post(this.url, { idReserva })
      .subscribe({ next: () => this.loadAll() });
  }

  adicionarItem(idComanda: number, idProduto: number, quantidade: number, observacao?: string): void {
    this.http.post(`${this.url}/${idComanda}/itens`, { idProduto, quantidade, observacao })
      .subscribe({ next: () => this.loadAll() });
  }

  removerItem(idComanda: number, idItem: number): void {
    this.http.delete(`${this.url}/${idComanda}/itens/${idItem}`)
      .subscribe({ next: () => this.loadAll() });
  }

  fecharComanda(idComanda: number): void {
    this.http.patch(`${this.url}/${idComanda}/fechar`, {})
      .subscribe({ next: () => this.loadAll() });
  }

  private mapComanda(c: any): Comanda {
    return {
      ...c,
      status: c.status?.toLowerCase() || 'aberta',
      dataAbertura: new Date(c.dataAbertura),
      dataFechamento: c.dataFechamento ? new Date(c.dataFechamento) : undefined,
      itens: (c.itens || []).map((i: any) => this.mapItem(i))
    };
  }

  private mapItem(i: any): ItemComanda {
    return {
      ...i,
      status: i.status?.toLowerCase() || 'pendente'
    };
  }

  private populateComanda(comanda: Comanda): Comanda {
    return {
      ...comanda,
      itens: comanda.itens.map(item => ({
        ...item,
        produto: this.produtoService.getById(item.idProduto),
      })),
    };
  }
}
