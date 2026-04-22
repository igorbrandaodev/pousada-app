import { Component, inject, computed, signal } from '@angular/core';
import { CurrencyPipe, DatePipe, PercentPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { DrawerModule } from 'primeng/drawer';
import { Dialog } from 'primeng/dialog';
import { Select } from 'primeng/select';
import { DatePicker } from 'primeng/datepicker';
import { Fluid } from 'primeng/fluid';
import { InputText } from 'primeng/inputtext';
import { Textarea } from 'primeng/textarea';
import { QuartoService } from '../../core/services/quarto.service';
import { ReservaService } from '../../core/services/reserva.service';
import { ComandaService } from '../../core/services/comanda.service';
import { HospedeService } from '../../core/services/hospede.service';
import { Reserva, Quarto, Comanda, Hospede } from '../../core/models';

interface StatusDistribution {
  label: string;
  count: number;
  color: string;
  percentage: number;
}

interface RoomDetail {
  quarto: Quarto;
  reserva?: Reserva;
  comandas: Comanda[];
  totalComandas: number;
  totalGeral: number;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CurrencyPipe, DatePipe, PercentPipe, FormsModule, CardModule, TagModule, ButtonModule, DrawerModule, Dialog, Select, DatePicker, Fluid, InputText, Textarea],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent {
  private readonly quartoService = inject(QuartoService);
  private readonly reservaService = inject(ReservaService);
  private readonly comandaService = inject(ComandaService);
  private readonly hospedeService = inject(HospedeService);
  private readonly router = inject(Router);

  // Drawer state
  protected drawerVisible = signal(false);
  protected roomDetail = signal<RoomDetail | null>(null);

  // New reservation dialog state
  protected newReservaVisible = signal(false);
  protected newReservaQuartoId = signal<number | null>(null);
  protected newReservaForm = { idHospede: null as number | null, dateRange: null as Date[] | null, observacoes: '' };
  protected today = new Date();

  // New hospede dialog state
  protected newHospedeVisible = signal(false);
  protected newHospedeForm: Partial<Hospede> = {};

  // KPIs
  protected readonly totalQuartos = computed(() => this.quartoService.quartos().length);
  protected readonly quartosOcupados = computed(() => this.quartoService.quartosOcupados().length);
  protected readonly ocupacaoPercent = computed(() => {
    const total = this.totalQuartos();
    return total > 0 ? this.quartosOcupados() / total : 0;
  });

  protected readonly hospedesNaPousada = computed(() =>
    this.reservaService.reservas()
      .filter(r => r.status === 'checkin')
      .map(r => ({ ...r, hospede: this.hospedeService.getById(r.idHospede), quarto: this.quartoService.getById(r.idQuarto) }))
  );

  protected readonly checkinsHoje = computed(() => {
    const today = this.todayStart();
    return this.reservaService.reservas()
      .filter(r => { const d = new Date(r.dataCheckin); d.setHours(0,0,0,0); return d.getTime() === today.getTime() && (r.status === 'pendente' || r.status === 'confirmada'); })
      .map(r => this.populateReserva(r));
  });

  protected readonly checkoutsHoje = computed(() => {
    const today = this.todayStart();
    return this.reservaService.reservas()
      .filter(r => { const d = new Date(r.dataCheckout); d.setHours(0,0,0,0); return d.getTime() === today.getTime() && r.status === 'checkin'; })
      .map(r => this.populateReserva(r));
  });

  protected readonly comandasAbertas = computed(() => this.comandaService.comandasAbertas());
  protected readonly totalComandas = computed(() => this.comandasAbertas().reduce((sum, c) => sum + c.total, 0));

  protected readonly proximosCheckins = computed(() => {
    const today = this.todayStart();
    const limit = new Date(today); limit.setDate(limit.getDate() + 7);
    return this.reservaService.reservas()
      .filter(r => { const d = new Date(r.dataCheckin); d.setHours(0,0,0,0); return d > today && d < limit && (r.status === 'pendente' || r.status === 'confirmada'); })
      .map(r => this.populateReserva(r))
      .sort((a, b) => new Date(a.dataCheckin).getTime() - new Date(b.dataCheckin).getTime());
  });

  protected readonly reservasPendentes = computed(() =>
    this.reservaService.reservas()
      .filter(r => r.status === 'pendente' || r.status === 'confirmada')
      .map(r => this.populateReserva(r))
      .sort((a, b) => new Date(a.dataCheckin).getTime() - new Date(b.dataCheckin).getTime())
  );

  protected readonly comandasAbertasDetalhadas = computed(() =>
    this.comandasAbertas().map(c => ({ ...c, reserva: this.reservaService.getById(c.idReserva) }))
  );

  protected readonly quartosMiniList = computed(() =>
    this.quartoService.quartos().map(q => {
      const reserva = q.status === 'ocupado' ? this.reservaService.reservas().find(r => r.idQuarto === q.id && r.status === 'checkin') : undefined;
      const hospede = reserva ? this.hospedeService.getById(reserva.idHospede) : undefined;
      return { id: q.id, numero: q.numero, status: q.status, guestName: hospede?.nome?.split(' ')[0] ?? null };
    }).sort((a, b) => a.numero.localeCompare(b.numero))
  );

  protected readonly restauranteMini = computed(() => {
    return this.hospedesNaPousada().map(r => {
      const comanda = this.comandaService.comandas().find(c => c.idReserva === r.id && c.status === 'aberta');
      return {
        quartoId: r.idQuarto,
        quartoNumero: r.quarto?.numero ?? '?',
        hospedeNome: r.hospede?.nome?.split(' ')[0] ?? '?',
        temComanda: !!comanda,
        comandaTotal: comanda?.total ?? 0,
      };
    });
  });

  protected readonly comandasFechadas = computed(() => {
    const today = this.todayStart();
    const tomorrow = new Date(today);
    tomorrow.setDate(tomorrow.getDate() + 1);

    return this.comandaService.comandas()
      .filter(c => {
        if (c.status !== 'fechada') return false;
        const closed = new Date(c.dataFechamento ?? c.dataAbertura);
        return closed >= today && closed < tomorrow;
      })
      .map(c => ({ ...c, reserva: this.reservaService.getById(c.idReserva) }))
      .sort((a, b) => new Date(b.dataFechamento ?? b.dataAbertura).getTime() - new Date(a.dataFechamento ?? a.dataAbertura).getTime());
  });

  protected readonly totalComandasFechadas = computed(() =>
    this.comandasFechadas().reduce((s, c) => s + c.total, 0)
  );

  protected readonly statusDistribution = computed((): StatusDistribution[] => {
    const quartos = this.quartoService.quartos();
    const total = quartos.length;
    if (total === 0) return [];
    const map: Record<Quarto['status'], { label: string; color: string }> = {
      disponivel: { label: 'Disponivel', color: 'var(--success)' }, ocupado: { label: 'Ocupado', color: 'var(--danger)' },
      limpeza: { label: 'Limpeza', color: 'var(--info)' }, manutencao: { label: 'Manutencao', color: 'var(--warning)' },
    };
    return (Object.keys(map) as Quarto['status'][])
      .map(s => ({ label: map[s].label, count: quartos.filter(q => q.status === s).length, color: map[s].color, percentage: 0 }))
      .filter(s => s.count > 0);
  });

  // For new reservation dialog
  protected readonly hospedeOptions = computed(() =>
    this.hospedeService.hospedes().map(h => ({ label: h.nome, value: h.id }))
  );

  protected readonly selectedQuartoLabel = computed(() => {
    const id = this.newReservaQuartoId();
    if (!id) return '';
    const q = this.quartoService.getById(id);
    return q ? `${q.numero} - ${q.tipo}` : '';
  });

  // Room click handler
  protected onRoomClick(quarto: { id: number; status: string }): void {
    if (quarto.status === 'ocupado') {
      this.openRoomDetail(quarto.id);
    } else if (quarto.status === 'disponivel') {
      this.openNewReserva(quarto.id);
    } else {
      this.router.navigate(['/quartos']);
    }
  }

  protected openRoomDetail(quartoId: number): void {
    const quarto = this.quartoService.getById(quartoId);
    if (!quarto) return;

    const reserva = this.reservaService.reservas().find(r => r.idQuarto === quartoId && r.status === 'checkin')
      ?? this.reservaService.reservas().find(r => r.idQuarto === quartoId && (r.status === 'pendente' || r.status === 'confirmada'))
      ?? this.reservaService.reservas().find(r => r.idQuarto === quartoId && r.status === 'checkout');
    const populatedReserva = reserva ? this.populateReserva(reserva) : undefined;

    const comandas = reserva
      ? this.comandaService.comandas().filter(c => c.idReserva === reserva.id).map(c => ({ ...c, itens: c.itens.map(i => ({ ...i, produto: this.comandaService.getById(c.id)?.itens.find(ci => ci.id === i.id)?.produto ?? i.produto })) }))
      : [];

    const totalComandas = comandas.reduce((s, c) => s + c.total, 0);

    this.roomDetail.set({
      quarto,
      reserva: populatedReserva,
      comandas,
      totalComandas,
      totalGeral: (populatedReserva?.valorTotal ?? 0) + totalComandas,
    });
    this.drawerVisible.set(true);
  }

  // Free room → new reservation dialog
  private openNewReserva(quartoId: number): void {
    this.newReservaQuartoId.set(quartoId);
    this.newReservaForm = { idHospede: null, dateRange: null, observacoes: '' };
    this.newReservaVisible.set(true);
  }

  protected saveNewReserva(): void {
    const quartoId = this.newReservaQuartoId();
    const range = this.newReservaForm.dateRange;
    if (!quartoId || !this.newReservaForm.idHospede || !range || !range[0] || !range[1]) return;

    this.reservaService.add({
      idHospede: this.newReservaForm.idHospede,
      idQuarto: quartoId,
      dataCheckin: range[0],
      dataCheckout: range[1],
      observacoes: this.newReservaForm.observacoes,
    });
    this.newReservaVisible.set(false);
  }

  protected confirmReserva(reservaId: number): void {
    this.reservaService.updateStatus(reservaId, 'confirmada');
    this.drawerVisible.set(false);
  }

  protected doCheckin(reservaId: number): void {
    this.reservaService.updateStatus(reservaId, 'checkin');
    this.drawerVisible.set(false);
  }

  protected doCheckout(reservaId: number): void {
    this.reservaService.updateStatus(reservaId, 'checkout');
    this.drawerVisible.set(false);
  }

  protected goToRestaurante(): void {
    const detail = this.roomDetail();
    this.drawerVisible.set(false);
    this.router.navigate(['/restaurante'], { queryParams: { quarto: detail?.quarto.id } });
  }

  protected openNewHospede(): void {
    this.newHospedeForm = {};
    this.newHospedeVisible.set(true);
  }

  protected saveNewHospede(): void {
    if (!this.newHospedeForm.nome) return;
    this.hospedeService.add(this.newHospedeForm);
    this.newHospedeVisible.set(false);
    setTimeout(() => {
      const hospedes = this.hospedeService.hospedes();
      const newest = hospedes[hospedes.length - 1];
      if (newest) this.newReservaForm.idHospede = newest.id;
    }, 500);
  }

  protected get isNewReservaValid(): boolean {
    const range = this.newReservaForm.dateRange;
    return !!(this.newReservaForm.idHospede && range && range[0] && range[1]);
  }

  protected getDateLabel(date: Date): string {
    if (this.isToday(date)) return 'Hoje';
    if (this.isTomorrow(date)) return 'Amanha';
    const d = new Date(date);
    return `${d.getDate().toString().padStart(2, '0')}/${(d.getMonth() + 1).toString().padStart(2, '0')}`;
  }

  protected daysUntilCheckout(date: Date): number {
    const today = this.todayStart();
    const checkout = new Date(date); checkout.setHours(0,0,0,0);
    return Math.ceil((checkout.getTime() - today.getTime()) / (1000 * 60 * 60 * 24));
  }

  protected isToday(date: Date): boolean {
    const t = new Date(); const d = new Date(date);
    return d.getFullYear() === t.getFullYear() && d.getMonth() === t.getMonth() && d.getDate() === t.getDate();
  }

  private isTomorrow(date: Date): boolean {
    const t = new Date(); t.setDate(t.getDate() + 1); const d = new Date(date);
    return d.getFullYear() === t.getFullYear() && d.getMonth() === t.getMonth() && d.getDate() === t.getDate();
  }

  protected navigateTo(route: string, queryParams?: Record<string, any>): void {
    this.router.navigate([route], queryParams ? { queryParams } : undefined);
  }

  protected goToRestauranteQuarto(quartoId: number): void {
    this.router.navigate(['/restaurante'], { queryParams: { quarto: quartoId } });
  }

  private todayStart(): Date { const d = new Date(); d.setHours(0,0,0,0); return d; }

  private populateReserva(r: Reserva): Reserva {
    return { ...r, hospede: this.hospedeService.getById(r.idHospede), quarto: this.quartoService.getById(r.idQuarto) };
  }
}
