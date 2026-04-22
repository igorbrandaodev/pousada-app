import { Component, inject, computed, signal, OnInit } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { InputText } from 'primeng/inputtext';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { ReservaService } from '../../core/services/reserva.service';
import { ComandaService } from '../../core/services/comanda.service';
import { ProdutoService } from '../../core/services/produto.service';
import { QuartoService } from '../../core/services/quarto.service';
import { HospedeService } from '../../core/services/hospede.service';
import { Reserva, Comanda, Produto } from '../../core/models';

interface QuartoOcupado {
  reserva: Reserva;
  quartoNumero: string;
  hospedeNome: string;
  idQuarto: number;
  comanda: Comanda | null;
}

interface CategoriaOption {
  label: string;
  value: number | null;
}

@Component({
  selector: 'app-restaurante',
  standalone: true,
  imports: [
    CurrencyPipe,
    FormsModule,
    TagModule,
    ButtonModule,
    InputText,
    ConfirmDialogModule,
  ],
  providers: [ConfirmationService],
  templateUrl: './restaurante.component.html',
  styleUrl: './restaurante.component.scss'
})
export class RestauranteComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly reservaService = inject(ReservaService);
  private readonly comandaService = inject(ComandaService);
  private readonly produtoService = inject(ProdutoService);
  private readonly quartoService = inject(QuartoService);
  private readonly hospedeService = inject(HospedeService);
  private readonly confirmationService = inject(ConfirmationService);

  protected selectedQuartoId = signal<number | null>(null);
  protected searchText = signal('');
  protected categoriaFilter = signal<number | null>(null);

  protected readonly quartosOcupados = computed<QuartoOcupado[]>(() => {
    return this.reservaService.reservas()
      .filter(r => r.status === 'checkin')
      .map(r => {
        const quarto = this.quartoService.getById(r.idQuarto);
        const hospede = this.hospedeService.getById(r.idHospede);
        const comandas = this.comandaService.comandas().filter(c => c.idReserva === r.id);
        const aberta = comandas.find(c => c.status === 'aberta');
        const ultima = comandas.sort((a, b) => new Date(b.dataAbertura).getTime() - new Date(a.dataAbertura).getTime())[0];
        return {
          reserva: r,
          quartoNumero: quarto?.numero ?? '?',
          hospedeNome: hospede?.nome ?? 'Hospede',
          idQuarto: r.idQuarto,
          comanda: aberta ?? ultima ?? null,
        };
      })
      .sort((a, b) => a.quartoNumero.localeCompare(b.quartoNumero));
  });

  protected readonly selectedQuarto = computed<QuartoOcupado | null>(() => {
    const id = this.selectedQuartoId();
    if (!id) return null;
    return this.quartosOcupados().find(q => q.idQuarto === id) ?? null;
  });

  protected readonly comandaAtiva = computed<Comanda | null>(() => {
    const sel = this.selectedQuarto();
    if (!sel?.comanda) return null;
    return this.comandaService.getById(sel.comanda.id) ?? null;
  });

  protected readonly categoriaOptions = computed<CategoriaOption[]>(() => [
    { label: 'Todas', value: null },
    ...this.produtoService.categorias().map(c => ({ label: c.nome, value: c.id })),
  ]);

  protected readonly produtosDisponiveis = computed<Produto[]>(() => {
    const search = this.searchText().toLowerCase();
    const cat = this.categoriaFilter();
    return this.produtoService.produtos().filter(p => {
      if (!p.disponivel) return false;
      if (cat !== null && p.idCategoria !== cat) return false;
      if (search && !p.nome.toLowerCase().includes(search)) return false;
      return true;
    });
  });

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      const quartoParam = params['quarto'];
      if (quartoParam) {
        const quartoId = Number(quartoParam);
        if (!isNaN(quartoId)) {
          this.selectedQuartoId.set(quartoId);
        }
      }
    });
  }

  protected selectQuarto(idQuarto: number): void {
    this.selectedQuartoId.set(idQuarto);
  }

  protected voltarParaLista(): void {
    this.selectedQuartoId.set(null);
  }

  protected abrirComanda(idReserva: number, idQuarto: number): void {
    this.comandaService.abrirComanda(idReserva);
    this.selectedQuartoId.set(idQuarto);
  }

  protected adicionarItem(produto: Produto): void {
    const comanda = this.comandaAtiva();
    if (!comanda) return;
    this.comandaService.adicionarItem(comanda.id, produto.id, 1);
  }

  protected removerItem(idItem: number): void {
    const comanda = this.comandaAtiva();
    if (!comanda) return;
    this.comandaService.removerItem(comanda.id, idItem);
  }

  protected fecharComanda(): void {
    const comanda = this.comandaAtiva();
    if (!comanda) return;
    this.confirmationService.confirm({
      message: 'Deseja fechar esta comanda?',
      header: 'Fechar Comanda',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Sim, fechar',
      rejectLabel: 'Cancelar',
      accept: () => this.comandaService.fecharComanda(comanda.id),
    });
  }

  protected onSearchChange(value: string): void {
    this.searchText.set(value);
  }

  protected onCategoriaChange(value: number | null): void {
    this.categoriaFilter.set(value);
  }
}
