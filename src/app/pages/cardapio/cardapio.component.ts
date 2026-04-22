import { Component, inject, signal, computed } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputText } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { DropdownModule } from 'primeng/dropdown';
import { Textarea } from 'primeng/textarea';
import { SelectButtonModule } from 'primeng/selectbutton';
import { ToggleSwitchModule } from 'primeng/toggleswitch';
import { ProdutoService } from '../../core/services/produto.service';
import { Produto } from '../../core/models';

@Component({
  selector: 'app-cardapio',
  standalone: true,
  imports: [
    CurrencyPipe,
    FormsModule,
    ButtonModule,
    DialogModule,
    InputText,
    InputNumberModule,
    DropdownModule,
    Textarea,
    SelectButtonModule,
    ToggleSwitchModule,
  ],
  templateUrl: './cardapio.component.html',
  styleUrl: './cardapio.component.scss'
})
export class CardapioComponent {
  private readonly produtoService = inject(ProdutoService);

  protected categoriaFilter = signal<number | null>(null);
  protected dialogVisible = signal(false);
  protected isNewProduto = signal(false);
  protected editingProduto = signal<Produto | null>(null);

  protected categorias = this.produtoService.categorias;

  protected categoriaOptions = computed(() => [
    { label: 'Todas', value: null },
    ...this.produtoService.categorias().map(c => ({ label: c.nome, value: c.id })),
  ]);

  protected categoriaDropdownOptions = computed(() =>
    this.produtoService.categorias().map(c => ({ label: c.nome, value: c.id }))
  );

  protected produtosFiltrados = computed(() => {
    const filter = this.categoriaFilter();
    const produtos = this.produtoService.produtos();
    if (filter === null) return produtos;
    return produtos.filter(p => p.idCategoria === filter);
  });

  protected getCategoriaLabel(idCategoria: number): string {
    return this.categorias().find(c => c.id === idCategoria)?.nome ?? '';
  }

  protected getCategoriaClass(idCategoria: number): string {
    const map: Record<number, string> = { 1: 'bebidas', 2: 'pratos', 3: 'petiscos', 4: 'sobremesas', 5: 'drinks' };
    return map[idCategoria] ?? '';
  }

  protected onFilterChange(value: number | null): void {
    this.categoriaFilter.set(value ?? null);
  }

  protected openNew(): void {
    this.editingProduto.set({
      id: 0,
      nome: '',
      descricao: '',
      preco: 0,
      categoria: '',
      idCategoria: this.produtoService.categorias()[0]?.id ?? 1,
      disponivel: true,
    });
    this.isNewProduto.set(true);
    this.dialogVisible.set(true);
  }

  protected openEdit(produto: Produto): void {
    this.editingProduto.set({ ...produto });
    this.isNewProduto.set(false);
    this.dialogVisible.set(true);
  }

  protected updateField(field: keyof Produto, value: any): void {
    const current = this.editingProduto();
    if (!current) return;
    this.editingProduto.set({ ...current, [field]: value });
  }

  protected save(): void {
    const p = this.editingProduto();
    if (!p) return;
    if (this.isNewProduto()) {
      this.produtoService.add(p);
    } else {
      this.produtoService.update(p);
    }
    this.dialogVisible.set(false);
  }

  protected cancel(): void {
    this.dialogVisible.set(false);
  }
}
