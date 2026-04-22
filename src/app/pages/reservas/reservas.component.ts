import { Component, inject, signal, computed, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TableModule, Table } from 'primeng/table';
import { Dialog } from 'primeng/dialog';
import { Select } from 'primeng/select';
import { DatePicker } from 'primeng/datepicker';
import { InputText } from 'primeng/inputtext';
import { Textarea } from 'primeng/textarea';
import { Button } from 'primeng/button';
import { Tag } from 'primeng/tag';
import { IconField } from 'primeng/iconfield';
import { InputIcon } from 'primeng/inputicon';
import { Fluid } from 'primeng/fluid';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { Tooltip } from 'primeng/tooltip';
import { ConfirmationService } from 'primeng/api';
import { CdkDropList, CdkDrag, CdkDragDrop } from '@angular/cdk/drag-drop';
import { ReservaService } from '../../core/services/reserva.service';
import { HospedeService } from '../../core/services/hospede.service';
import { QuartoService } from '../../core/services/quarto.service';
import { Reserva, Hospede, Quarto } from '../../core/models';

interface StatusOption {
  label: string;
  value: string;
}

type ReservaStatus = Reserva['status'];

@Component({
  selector: 'app-reservas',
  standalone: true,
  imports: [
    FormsModule,
    TableModule,
    Dialog,
    Select,
    DatePicker,
    InputText,
    Textarea,
    Button,
    Tag,
    IconField,
    InputIcon,
    Fluid,
    ConfirmDialog,
    Tooltip,
    CdkDropList,
    CdkDrag,
  ],
  providers: [ConfirmationService],
  templateUrl: './reservas.component.html',
  styleUrl: './reservas.component.scss',
})
export class ReservasComponent {
  private readonly reservaService = inject(ReservaService);
  private readonly hospedeService = inject(HospedeService);
  private readonly quartoService = inject(QuartoService);
  private readonly confirmationService = inject(ConfirmationService);

  protected readonly statusOptions: StatusOption[] = [
    { label: 'Todas', value: 'todas' },
    { label: 'Pendente', value: 'pendente' },
    { label: 'Confirmada', value: 'confirmada' },
    { label: 'Check-in', value: 'checkin' },
    { label: 'Check-out', value: 'checkout' },
    { label: 'Cancelada', value: 'cancelada' },
  ];

  protected statusFilter = signal('todas');
  protected periodFilter = signal<Date[] | null>(null);
  protected searchValue = signal('');
  protected dialogVisible = signal(false);
  protected editing = signal(false);
  protected hospedeDialogVisible = signal(false);
  protected newHospede: Partial<Hospede> = {};

  protected hospedesList = this.hospedeService.hospedes;
  protected quartosList = this.quartoService.quartos;

  protected today = new Date();
  protected reservaForm: Partial<Reserva> & { dateRange: Date[] | null } = this.emptyReserva();

  @ViewChild('dt') private dt!: Table;

  protected availableQuartos = computed(() => {
    const quartos = this.quartoService.quartosDisponiveis();
    if (this.editing() && this.reservaForm.idQuarto) {
      const current = this.quartoService.getById(this.reservaForm.idQuarto);
      if (current && !quartos.find(q => q.id === current.id)) {
        return [...quartos, current];
      }
    }
    return quartos;
  });

  protected quartoOptions = computed(() =>
    this.availableQuartos().map(q => ({
      label: `${q.numero} - ${this.tipoLabel(q.tipo)} (R$ ${q.precoDiaria}/noite)`,
      value: q.id,
    }))
  );

  protected hospedeOptions = computed(() =>
    this.hospedesList().map(h => ({
      label: h.nome,
      value: h.id,
    }))
  );

  protected filteredReservas = computed(() => {
    let reservas = this.reservaService.reservas().map(r => ({
      ...r,
      hospede: this.hospedeService.getById(r.idHospede),
      quarto: this.quartoService.getById(r.idQuarto),
    }));

    const status = this.statusFilter();
    if (status !== 'todas') {
      reservas = reservas.filter(r => r.status === status);
    }

    const period = this.periodFilter();
    if (period && period[0]) {
      const start = new Date(period[0]);
      start.setHours(0, 0, 0, 0);
      reservas = reservas.filter(r => {
        const checkin = new Date(r.dataCheckin);
        checkin.setHours(0, 0, 0, 0);
        return checkin >= start;
      });
      if (period[1]) {
        const end = new Date(period[1]);
        end.setHours(23, 59, 59, 999);
        reservas = reservas.filter(r => {
          const checkin = new Date(r.dataCheckin);
          return checkin <= end;
        });
      }
    }

    const search = this.searchValue().toLowerCase();
    if (search) {
      reservas = reservas.filter(r =>
        r.hospede?.nome.toLowerCase().includes(search) ||
        r.quarto?.numero.includes(search) ||
        r.id.toString().includes(search)
      );
    }

    return reservas;
  });

  protected kanbanColumns = computed(() => {
    const all = this.filteredReservas();
    return [
      { status: 'pendente' as ReservaStatus, label: 'Pendente', color: 'var(--warning)', items: all.filter(r => r.status === 'pendente') },
      { status: 'confirmada' as ReservaStatus, label: 'Confirmada', color: 'var(--success)', items: all.filter(r => r.status === 'confirmada') },
      { status: 'checkin' as ReservaStatus, label: 'Check-in', color: 'var(--info)', items: all.filter(r => r.status === 'checkin') },
      { status: 'checkout' as ReservaStatus, label: 'Check-out', color: 'var(--checkout-color)', items: all.filter(r => r.status === 'checkout') },
    ];
  });

  protected readonly columnIds = ['col-pendente', 'col-confirmada', 'col-checkin', 'col-checkout'];

  protected onDrop(event: CdkDragDrop<any[]>, targetStatus: ReservaStatus): void {
    const reserva = event.item.data as Reserva;
    if (reserva.status === targetStatus) return;

    if (targetStatus === 'checkin') {
      this.fazerCheckin(reserva);
    } else if (targetStatus === 'checkout') {
      this.fazerCheckout(reserva);
    } else if (targetStatus === 'confirmada') {
      this.confirmarReserva(reserva);
    } else if (targetStatus === 'pendente') {
      this.reservaService.updateStatus(reserva.id, 'pendente');
    }
  }

  protected get nightsCount(): number {
    const range = this.reservaForm.dateRange;
    if (!range || !range[0] || !range[1]) return 0;
    const diff = range[1].getTime() - range[0].getTime();
    return Math.max(0, Math.ceil(diff / (1000 * 60 * 60 * 24)));
  }

  protected get dailyRate(): number {
    if (!this.reservaForm.idQuarto) return 0;
    const quarto = this.quartoService.getById(this.reservaForm.idQuarto);
    return quarto?.precoDiaria ?? 0;
  }

  protected get calculatedTotal(): number {
    return this.nightsCount * this.dailyRate;
  }

  private emptyReserva(): Partial<Reserva> & { dateRange: Date[] | null } {
    return {
      id: 0,
      idHospede: undefined,
      idQuarto: undefined,
      dateRange: null,
      status: 'pendente',
      valorTotal: 0,
      observacoes: '',
    };
  }

  protected openNew(): void {
    this.reservaForm = this.emptyReserva();
    this.editing.set(false);
    this.dialogVisible.set(true);
  }

  protected editReserva(reserva: Reserva): void {
    this.reservaForm = {
      ...reserva,
      dateRange: [new Date(reserva.dataCheckin), new Date(reserva.dataCheckout)],
    };
    this.editing.set(true);
    this.dialogVisible.set(true);
  }

  protected saveReserva(): void {
    const range = this.reservaForm.dateRange;
    if (!this.reservaForm.idHospede || !this.reservaForm.idQuarto || !range || !range[0] || !range[1]) return;

    const reserva: Reserva = {
      id: this.reservaForm.id ?? 0,
      idHospede: this.reservaForm.idHospede,
      idQuarto: this.reservaForm.idQuarto,
      dataCheckin: range[0],
      dataCheckout: range[1],
      status: this.reservaForm.status as ReservaStatus ?? 'pendente',
      valorTotal: this.calculatedTotal,
      observacoes: this.reservaForm.observacoes,
    };

    if (this.editing()) {
      this.reservaService.update(reserva);
    } else {
      const { id, ...data } = reserva;
      this.reservaService.add(data);
    }
    this.dialogVisible.set(false);
  }

  protected confirmarReserva(reserva: Reserva): void {
    this.reservaService.updateStatus(reserva.id, 'confirmada');
  }

  protected fazerCheckin(reserva: Reserva): void {
    this.reservaService.updateStatus(reserva.id, 'checkin');
    this.quartoService.updateStatus(reserva.idQuarto, 'ocupado');
  }

  protected fazerCheckout(reserva: Reserva): void {
    this.reservaService.updateStatus(reserva.id, 'checkout');
    this.quartoService.updateStatus(reserva.idQuarto, 'limpeza');
  }

  protected cancelarReserva(reserva: Reserva): void {
    this.confirmationService.confirm({
      message: `Deseja cancelar a reserva #${reserva.id}?`,
      header: 'Confirmar Cancelamento',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Sim, cancelar',
      rejectLabel: 'Não',
      accept: () => {
        this.reservaService.updateStatus(reserva.id, 'cancelada');
        if (reserva.status === 'checkin') {
          this.quartoService.updateStatus(reserva.idQuarto, 'limpeza');
        }
      },
    });
  }

  protected openNewHospede(): void {
    this.newHospede = {};
    this.hospedeDialogVisible.set(true);
  }

  protected saveNewHospede(): void {
    if (!this.newHospede.nome) return;
    this.hospedeService.add(this.newHospede);
    this.hospedeDialogVisible.set(false);
    // After a short delay for the API to respond and loadAll to complete, select the new hospede
    setTimeout(() => {
      const hospedes = this.hospedeService.hospedes();
      const newest = hospedes[hospedes.length - 1];
      if (newest) {
        this.reservaForm.idHospede = newest.id;
      }
    }, 500);
  }

  protected statusSeverity(status: ReservaStatus): "success" | "info" | "warn" | "danger" | "secondary" | "contrast" | undefined {
    const map: Record<ReservaStatus, "success" | "info" | "warn" | "danger" | "secondary"> = {
      pendente: 'warn',
      confirmada: 'success',
      checkin: 'info',
      checkout: 'secondary',
      cancelada: 'danger',
    };
    return map[status];
  }

  protected statusLabel(status: ReservaStatus): string {
    const map: Record<ReservaStatus, string> = {
      pendente: 'Pendente',
      confirmada: 'Confirmada',
      checkin: 'Check-in',
      checkout: 'Check-out',
      cancelada: 'Cancelada',
    };
    return map[status];
  }

  protected tipoLabel(tipo: Quarto['tipo']): string {
    const map: Record<Quarto['tipo'], string> = {
      standard: 'Standard',
      luxo: 'Luxo',
      suite: 'Suíte',
    };
    return map[tipo];
  }

  protected formatDate(date: Date): string {
    return new Date(date).toLocaleDateString('pt-BR');
  }

  protected formatCurrency(value: number): string {
    return value.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
  }

  protected onGlobalFilter(event: Event): void {
    this.searchValue.set((event.target as HTMLInputElement).value);
  }

  protected onStatusChange(): void {
    this.statusFilter.set(this.statusFilter());
  }

  protected get isFormValid(): boolean {
    const range = this.reservaForm.dateRange;
    return !!(
      this.reservaForm.idHospede &&
      this.reservaForm.idQuarto &&
      range && range[0] && range[1] &&
      this.nightsCount > 0
    );
  }
}
