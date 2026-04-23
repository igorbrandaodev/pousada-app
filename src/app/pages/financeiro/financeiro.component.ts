import { Component, inject, computed, signal } from '@angular/core';
import { CurrencyPipe, DatePipe, DecimalPipe } from '@angular/common';
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

interface Variacao {
  percent: number;
  positive: boolean;
  hasData: boolean;
}

@Component({
  selector: 'app-financeiro',
  standalone: true,
  imports: [CurrencyPipe, DatePipe, DecimalPipe, FormsModule, CardModule, ChartModule, TableModule, TagModule, ButtonModule, SelectButtonModule, Select],
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

  // Período anterior para comparação (mesma duração imediatamente antes do período atual)
  protected readonly previousTransacoes = computed<Transacao[]>(() => {
    const days = this.periodoFilter();
    if (days <= 0) return [];

    const start = new Date();
    start.setDate(start.getDate() - days * 2);
    start.setHours(0, 0, 0, 0);
    const end = new Date();
    end.setDate(end.getDate() - days);
    end.setHours(0, 0, 0, 0);

    let items = this.allTransacoes().filter(t => t.data >= start && t.data < end);
    const tipo = this.tipoFilter();
    if (tipo !== 'todos') items = items.filter(t => t.tipo === tipo);
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

  // Totais período anterior
  private readonly prevTotalHospedagem = computed(() =>
    this.previousTransacoes().filter(t => t.tipo === 'hospedagem').reduce((s, t) => s + t.valor, 0)
  );
  private readonly prevTotalRestaurante = computed(() =>
    this.previousTransacoes().filter(t => t.tipo === 'restaurante').reduce((s, t) => s + t.valor, 0)
  );
  private readonly prevTotal = computed(() => this.prevTotalHospedagem() + this.prevTotalRestaurante());
  private readonly prevAvgStay = computed(() => {
    const stays = this.previousTransacoes().filter(t => t.tipo === 'hospedagem');
    return stays.length > 0 ? stays.reduce((s, t) => s + t.valor, 0) / stays.length : 0;
  });

  // Variação % vs período anterior
  private calcVariacao(current: number, previous: number): Variacao {
    // No previous baseline -> variação é indefinida (mostra como sem comparativo)
    if (previous === 0) return { percent: 0, positive: true, hasData: false };
    const delta = ((current - previous) / previous) * 100;
    return { percent: Math.abs(delta), positive: delta >= 0, hasData: true };
  }

  protected readonly varTotal = computed(() => this.calcVariacao(this.total(), this.prevTotal()));
  protected readonly varHospedagem = computed(() => this.calcVariacao(this.totalHospedagem(), this.prevTotalHospedagem()));
  protected readonly varRestaurante = computed(() => this.calcVariacao(this.totalRestaurante(), this.prevTotalRestaurante()));
  protected readonly varTicket = computed(() => this.calcVariacao(this.avgStayValue(), this.prevAvgStay()));

  protected readonly periodoLabel = computed(() => {
    const d = this.periodoFilter();
    return this.periodoOptions.find(o => o.value === d)?.label ?? '';
  });

  // Gráfico de barras empilhadas: receita por dia separada por tipo
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
        {
          label: 'Hospedagem',
          data: labels.map(l => byDay[l].hosp),
          backgroundColor: '#4A8FD9',
          borderRadius: 4,
          borderSkipped: false,
          barPercentage: 0.7,
          categoryPercentage: 0.8,
        },
        {
          label: 'Restaurante',
          data: labels.map(l => byDay[l].rest),
          backgroundColor: '#D49B0D',
          borderRadius: 4,
          borderSkipped: false,
          barPercentage: 0.7,
          categoryPercentage: 0.8,
        },
      ],
    };
  });

  protected readonly barChartOptions = computed(() => {
    this.filteredTransacoes();
    return {
      responsive: true,
      maintainAspectRatio: true,
      aspectRatio: 2,
      plugins: {
        legend: {
          position: 'bottom' as const,
          labels: { usePointStyle: true, font: { size: 11 }, padding: 12 },
        },
        tooltip: {
          backgroundColor: 'rgba(15, 23, 42, 0.95)',
          padding: 10,
          cornerRadius: 6,
          callbacks: {
            label: (ctx: { dataset: { label?: string }; parsed: { y: number } }) =>
              `${ctx.dataset.label}: R$ ${ctx.parsed.y.toLocaleString('pt-BR', { minimumFractionDigits: 2 })}`,
          },
        },
      },
      scales: {
        x: { stacked: true, grid: { display: false }, ticks: { font: { size: 10 } } },
        y: {
          stacked: true,
          grid: { color: 'rgba(0, 0, 0, 0.04)' },
          ticks: { callback: (v: number) => `R$${v}`, font: { size: 10 } },
        },
      },
    };
  });

  // Gráfico de área com gradient verde para receita acumulada
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
        backgroundColor: (ctx: { chart: { ctx: CanvasRenderingContext2D; chartArea?: { top: number; bottom: number } } }) => {
          const chart = ctx.chart;
          const { ctx: canvas, chartArea } = chart;
          if (!chartArea) return 'rgba(58, 125, 68, 0.2)';
          const gradient = canvas.createLinearGradient(0, chartArea.top, 0, chartArea.bottom);
          gradient.addColorStop(0, 'rgba(58, 125, 68, 0.35)');
          gradient.addColorStop(1, 'rgba(58, 125, 68, 0.02)');
          return gradient;
        },
        fill: true,
        tension: 0.35,
        pointRadius: 3,
        pointHoverRadius: 6,
        pointBackgroundColor: '#3A7D44',
        pointBorderColor: '#ffffff',
        pointBorderWidth: 2,
        borderWidth: 2.5,
      }],
    };
  });

  protected readonly lineChartOptions = computed(() => {
    this.filteredTransacoes();
    return {
      responsive: true,
      maintainAspectRatio: true,
      aspectRatio: 2,
      plugins: {
        legend: { display: false },
        tooltip: {
          backgroundColor: 'rgba(15, 23, 42, 0.95)',
          padding: 10,
          cornerRadius: 6,
          callbacks: {
            label: (ctx: { parsed: { y: number } }) =>
              `Acumulado: R$ ${ctx.parsed.y.toLocaleString('pt-BR', { minimumFractionDigits: 2 })}`,
          },
        },
      },
      scales: {
        x: { grid: { display: false }, ticks: { font: { size: 10 } } },
        y: {
          grid: { color: 'rgba(0, 0, 0, 0.04)' },
          ticks: { callback: (v: number) => `R$${v}`, font: { size: 10 } },
        },
      },
    };
  });

  protected getTagSeverity(tipo: Transacao['tipo']): 'info' | 'warn' {
    return tipo === 'hospedagem' ? 'info' : 'warn';
  }

  protected getTagLabel(tipo: Transacao['tipo']): string {
    return tipo === 'hospedagem' ? 'Hospedagem' : 'Restaurante';
  }
}
