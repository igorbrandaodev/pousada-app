import { Injectable, signal, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Produto, CategoriaCardapio } from '../models';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ProdutoService {
  private readonly http = inject(HttpClient);
  private readonly prodUrl = `${environment.apiUrl}/Produtos`;
  private readonly catUrl = `${environment.apiUrl}/Categorias`;

  private _produtos = signal<Produto[]>([]);
  private _categorias = signal<CategoriaCardapio[]>([]);

  public produtos = this._produtos.asReadonly();
  public categorias = this._categorias.asReadonly();

  constructor() {
    this.loadAll();
    this.loadCategorias();
  }

  loadAll(): void {
    this.http.get<any[]>(this.prodUrl).subscribe({
      next: (data) => this._produtos.set(data.map(p => ({
        ...p,
        categoria: p.categoria || '',
        idCategoria: p.idCategoria || 0
      }))),
      error: (err) => console.error('Failed to load produtos:', err)
    });
  }

  loadCategorias(): void {
    this.http.get<CategoriaCardapio[]>(this.catUrl).subscribe({
      next: (data) => this._categorias.set(data),
      error: (err) => console.error('Failed to load categorias:', err)
    });
  }

  getById(id: number): Produto | undefined {
    return this._produtos().find(p => p.id === id);
  }

  add(produto: Partial<Produto>): void {
    this.http.post(this.prodUrl, {
      idCategoria: produto.idCategoria,
      nome: produto.nome,
      descricao: produto.descricao,
      preco: produto.preco,
      fotoUrl: produto.imagem
    }).subscribe({ next: () => this.loadAll() });
  }

  update(produto: Produto): void {
    this.http.put(`${this.prodUrl}/${produto.id}`, {
      idCategoria: produto.idCategoria,
      nome: produto.nome,
      descricao: produto.descricao,
      preco: produto.preco,
      fotoUrl: produto.imagem,
      disponivel: produto.disponivel
    }).subscribe({ next: () => this.loadAll() });
  }

  delete(id: number): void {
    this.http.delete(`${this.prodUrl}/${id}`)
      .subscribe({ next: () => this.loadAll() });
  }

  getByCategoria(idCategoria: number): Produto[] {
    return this._produtos().filter(p => p.idCategoria === idCategoria);
  }
}
