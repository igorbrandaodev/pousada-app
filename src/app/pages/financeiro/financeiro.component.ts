import { Component, inject, computed, signal } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { ChartModule } from 'primeng/chart';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { SelectButtonModule } from 'primeng/selectbutton';
import { Select } from 'primeng/select';
import { ReservaService } from '../../core/services/reserva.service';
import { ComandaService } from '../../core/services/comanda.service';
import { HospedeService } from '../../core/services/hospede.service';
import { QuartoService } from '../../core/services/quarto.service';

interface Transacao {
  data: Date;
  descricao: string;
  tipo: 'hospedagem' | 'restaurante';
  valor: number;
}

@Component({
  selector: 'app-financeiro',
  standalone: true,
  imports: [CurrencyPipe, DatePipe, FormsModule, CardModule, ChartModule, TableModule, TagModule, ButtonModule, SelectButtonModule, Select],
  templateUrl: './financeiro.component.html',
  styleUrl: './financeiro.component.scss'
})
export class FinanceiroComponent {
  private readonly reservaService = inject(ReservaService);
  private readonly comandaService = inject(ComandaService);
  private readonly hospedeService = inject(HospedeService);
  private readonly quartoService = inject(QuartoService);

  protected tipoFilter = signal('todos');
  protected periodoFilter = signal(30);

  protected readonly tipoOptions = [
    { label: 'Todos', value: 'todos' },
    { label: 'Hospedagem', value: 'hospedagem' },
    { label: 'Restaurante', value: 'restaurante' },
  ];

  protected readonly periodoOptions = [
    { label: 'Hoje', value: 1 },
    { label: '7 dias', value: 7 },
    { label: '30 dias', value: 30 },
    { label: '90 dias', value: 90 },
    { label: 'Ano', value: 365 },
    { label: 'Tudo', value: 0 },
  ];

  protected readonly allTransacoes = computed<Transacao[]>(() => {
    const reservaTransacoes: Transacao[] = this.reservaService.reservas()
      .filter(r => r.status === 'checkout')
      .map(r => {
        const hospede = this.hospedeService.getById(r.idHospede);
        const quarto = this.quartoService.getById(r.idQuarto);
        return {
          data: new Date(r.dataCheckout),
          descricao: `Qto ${quarto?.numero ?? '?'} - ${hospede?.nome ?? 'Hospede'}`,
          tipo: 'hospedagem' as const,
          valor: r.valorTotal,
        };
      });

    const comandaTransacoes: Transacao[] = this.comandaService.comandas()
      .filter(c => c.status === 'fechada')
      .map(c => {
        const reserva = this.reservaService.getById(c.idReserva);
        return {
          data: new Date(c.dataFechamento ?? c.dataAbertura),
          descricao: `Comanda Qto ${reserva?.quarto?.numero ?? '?'} - ${reserva?.hospede?.nome ?? 'Hospede'}`,
          tipo: 'restaurante' as const,
          valor: c.total,
        };
      });

    return [...reservaTransacoes, ...comandaTransacoes]
      .sort((a, b) => b.data.getTime() - a.data.getTime());
  });

  protected readonly filteredTransacoes = computed(() => {
    let items = this.allTransacoes();

    const tipo = this.tipoFilter();
    if (tipo !== 'todos') {
      items = items.filter(t => t.tipo === tipo);
    }

    const days = this.periodoFilter();
    if (days > 0) {
      const cutoff = new Date();
      cutoff.setDate(cutoff.getDate() - days);
      cutoff.setHours(0, 0, 0, 0);
      items = items.filter(t => t.data >= cutoff);
    }

    return items;
  });

  protected readonly totalHospedagem = computed(() =>
    this.filteredTransacoes().filter(t => t.tipo === 'hospedagem').reduce((sum, t) => sum + t.valor, 0)
  );

  protected readonly totalRestaurante = computed(() =>
    this.filteredTransacoes().filter(t => t.tipo === 'restaurante').reduce((sum, t) => sum + t.valor, 0)
  );

  protected readonly total = computed(() => this.totalHospedagem() + this.totalRestaurante());

  protected readonly avgStayValue = computed(() => {
    const stays = this.filteredTransacoes().filter(t => t.tipo === 'hospedagem');
    return stays.length > 0 ? stays.reduce((s, t) => s + t.valor, 0) / stays.length : 0;
  });

  protected readonly totalCheckouts = computed(() =>
    this.filteredTransacoes().filter(t => t.tipo === 'hospedagem').length
  );

  protected readonly totalComandasFechadas = computed(() =>
    this.filteredTransacoes().filter(t => t.tipo === 'restaurante').length
  );

  protected readonly periodoLabel = computed(() => {
    const d = this.periodoFilter();
    return this.periodoOptions.find(o => o.value === d)?.label ?? '';
  });

  protected readonly barChartData = computed(() => {
    const items = this.filteredTransacoes();
    const byDay: Record<string, { hosp: number; rest: number }> = {};
    items.forEach(t => {
      const key = t.data.toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit' });
      if (!byDay[key]) byDay[key] = { hosp: 0, rest: 0 };
      if (t.tipo === 'hospedagem') byDay[key].hosp += t.valor;
      else byDay[key].rest += t.valor;
    });
    const labels = Object.keys(byDay);
    return {
      labels,
      datasets: [
        { label: 'Hospedagem', data: labels.map(l => byDay[l].hosp), backgroundColor: '#B7622B' },
        { label: 'Restaurante', data: labels.map(l => byDay[l].rest), backgroundColor: '#D49B0D' },
      ],
    };
  });

  protected readonly barChartOptions = computed(() => {
    this.filteredTransacoes();
    return {
      responsive: true,
      maintainAspectRatio: false,
      plugins: { legend: { position: 'bottom' as const, labels: { usePointStyle: true, font: { size: 11 } } } },
      scales: { x: { stacked: true }, y: { stacked: true, ticks: { callback: (v: number) => `R$${v}` } } },
    };
  });

  protected readonly lineChartData = computed(() => {
    const items = this.filteredTransacoes();
    const byDay: Record<string, number> = {};
    items.forEach(t => {
      const key = t.data.toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit' });
      byDay[key] = (byDay[key] || 0) + t.valor;
    });
    const labels = Object.keys(byDay).reverse();
    const values = labels.map(l => byDay[l]);
    const cumulative: number[] = [];
    values.forEach((v, i) => cumulative.push((cumulative[i - 1] || 0) + v));
    return {
      labels,
      datasets: [{
        label: 'Receita Acumulada',
        data: cumulative,
        borderColor: '#3A7D44',
        backgroundColor: 'rgba(58, 125, 68, 0.1)',
        fill: true,
        tension: 0.3,
        pointRadius: 3,
        pointBackgroundColor: '#3A7D44',
      }],
    };
  });

  protected readonly lineChartOptions = computed(() => {
    this.filteredTransacoes(); // trigger reactivity
    return {
      responsive: true,
      maintainAspectRatio: false,
      plugins: { legend: { display: false } },
      scales: { y: { ticks: { callback: (v: number) => `R$${v}` } } },
    };
  });

  protected getTagSeverity(tipo: Transacao['tipo']): 'info' | 'warn' {
    return tipo === 'hospedagem' ? 'info' : 'warn';
  }

  protected getTagLabel(tipo: Transacao['tipo']): string {
    return tipo === 'hospedagem' ? 'Hospedagem' : 'Restaurante';
  }
}
