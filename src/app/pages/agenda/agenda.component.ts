import { Component, inject, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { SelectButtonModule } from 'primeng/selectbutton';
import { TagModule } from 'primeng/tag';
import { QuartoService } from '../../core/services/quarto.service';
import { ReservaService } from '../../core/services/reserva.service';
import { HospedeService } from '../../core/services/hospede.service';
import { Reserva, Quarto } from '../../core/models';

type ViewMode = 'mes' | 'semana' | 'dia';

interface DayCell {
  date: Date;
  dayNum: number;
  currentMonth: boolean;
  isToday: boolean;
  reservas: Reserva[];
}

interface WeekDay {
  date: Date;
  label: string;
  dayNum: number;
  isToday: boolean;
}

interface DayItem {
  quarto: Quarto;
  reserva?: Reserva;
}

@Component({
  selector: 'app-agenda',
  standalone: true,
  imports: [FormsModule, ButtonModule, SelectButtonModule, TagModule],
  templateUrl: './agenda.component.html',
  styleUrl: './agenda.component.scss'
})
export class AgendaComponent {
  private readonly quartoService = inject(QuartoService);
  private readonly reservaService = inject(ReservaService);
  private readonly hospedeService = inject(HospedeService);
  private readonly router = inject(Router);

  protected readonly currentDate = signal(new Date());
  protected readonly viewMode = signal<ViewMode>('mes');

  protected readonly weekdayLabels = ['Dom', 'Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sab'];

  protected readonly viewOptions = [
    { label: 'Mes', value: 'mes' },
    { label: 'Semana', value: 'semana' },
    { label: 'Dia', value: 'dia' },
  ];

  protected quartos = computed(() => this.quartoService.quartos());

  protected headerLabel = computed(() => {
    const d = this.currentDate();
    const month = d.toLocaleDateString('pt-BR', { month: 'long', year: 'numeric' });
    if (this.viewMode() === 'dia') {
      return d.toLocaleDateString('pt-BR', { day: '2-digit', month: 'long', year: 'numeric' });
    }
    return month.charAt(0).toUpperCase() + month.slice(1);
  });

  protected monthDays = computed<DayCell[]>(() => {
    const d = this.currentDate();
    const year = d.getFullYear();
    const month = d.getMonth();
    const firstDay = new Date(year, month, 1);
    const lastDay = new Date(year, month + 1, 0);
    const startWeekday = firstDay.getDay();
    const daysInMonth = lastDay.getDate();

    const cells: DayCell[] = [];
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    // Previous month tail
    for (let i = startWeekday - 1; i >= 0; i--) {
      const date = new Date(year, month, -i);
      cells.push(this.makeCell(date, false, today));
    }
    // Current month
    for (let day = 1; day <= daysInMonth; day++) {
      const date = new Date(year, month, day);
      cells.push(this.makeCell(date, true, today));
    }
    // Next month head to fill 6 rows (42 cells)
    while (cells.length < 42) {
      const date = new Date(year, month + 1, cells.length - daysInMonth - startWeekday + 1);
      cells.push(this.makeCell(date, false, today));
    }
    return cells;
  });

  protected weekDays = computed<WeekDay[]>(() => {
    const d = this.currentDate();
    const start = new Date(d);
    start.setDate(d.getDate() - d.getDay());
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    return Array.from({ length: 7 }, (_, i) => {
      const date = new Date(start);
      date.setDate(start.getDate() + i);
      return {
        date,
        label: this.weekdayLabels[i],
        dayNum: date.getDate(),
        isToday: date.getTime() === today.getTime(),
      };
    });
  });

  protected dayItems = computed<DayItem[]>(() => {
    const target = new Date(this.currentDate());
    target.setHours(0, 0, 0, 0);
    return this.quartoService.quartos().map(q => {
      const reserva = this.reservaService.reservas()
        .map(r => ({ ...r, hospede: this.hospedeService.getById(r.idHospede) }))
        .find(r => {
          if (r.idQuarto !== q.id) return false;
          if (r.status === 'cancelada') return false;
          const ci = new Date(r.dataCheckin); ci.setHours(0, 0, 0, 0);
          const co = new Date(r.dataCheckout); co.setHours(0, 0, 0, 0);
          return target >= ci && target <= co;
        });
      return { quarto: q, reserva };
    });
  });

  private makeCell(date: Date, currentMonth: boolean, today: Date): DayCell {
    const d = new Date(date); d.setHours(0, 0, 0, 0);
    const reservas = this.reservaService.reservas()
      .map(r => ({ ...r, hospede: this.hospedeService.getById(r.idHospede) }))
      .filter(r => {
        if (r.status === 'cancelada') return false;
        const ci = new Date(r.dataCheckin); ci.setHours(0, 0, 0, 0);
        const co = new Date(r.dataCheckout); co.setHours(0, 0, 0, 0);
        return d >= ci && d <= co;
      });
    return {
      date,
      dayNum: date.getDate(),
      currentMonth,
      isToday: d.getTime() === today.getTime(),
      reservas,
    };
  }

  protected getQuartoNum(quartoId: number): string {
    return this.quartoService.getById(quartoId)?.numero ?? '?';
  }

  protected getReservasForRoomDate(quartoId: number, date: Date): Reserva[] {
    const target = new Date(date); target.setHours(0, 0, 0, 0);
    return this.reservaService.reservas()
      .map(r => ({ ...r, hospede: this.hospedeService.getById(r.idHospede) }))
      .filter(r => {
        if (r.idQuarto !== quartoId) return false;
        if (r.status === 'cancelada') return false;
        const ci = new Date(r.dataCheckin); ci.setHours(0, 0, 0, 0);
        const co = new Date(r.dataCheckout); co.setHours(0, 0, 0, 0);
        return target >= ci && target <= co;
      });
  }

  protected navigatePrev(): void {
    const d = new Date(this.currentDate());
    switch (this.viewMode()) {
      case 'mes': d.setMonth(d.getMonth() - 1); break;
      case 'semana': d.setDate(d.getDate() - 7); break;
      case 'dia': d.setDate(d.getDate() - 1); break;
    }
    this.currentDate.set(d);
  }

  protected navigateNext(): void {
    const d = new Date(this.currentDate());
    switch (this.viewMode()) {
      case 'mes': d.setMonth(d.getMonth() + 1); break;
      case 'semana': d.setDate(d.getDate() + 7); break;
      case 'dia': d.setDate(d.getDate() + 1); break;
    }
    this.currentDate.set(d);
  }

  protected goToToday(): void {
    this.currentDate.set(new Date());
  }

  protected selectDay(date: Date): void {
    this.currentDate.set(date);
    this.viewMode.set('dia');
  }

  protected goToComanda(quartoId: number): void {
    this.router.navigate(['/restaurante'], { queryParams: { quarto: quartoId } });
  }

  protected goToReservar(quartoId: number): void {
    this.router.navigate(['/reservas']);
  }

  protected onPillClick(reserva: Reserva): void {
    if (reserva.status === 'checkin') {
      // Active stay → manage comanda
      this.router.navigate(['/restaurante'], { queryParams: { quarto: reserva.idQuarto } });
    } else {
      // Future/past reservation → kanban
      this.router.navigate(['/reservas']);
    }
  }

  protected getStatusSeverity(status: Reserva['status']): 'success' | 'info' | 'warn' | 'danger' | 'secondary' {
    const map: Record<Reserva['status'], 'success' | 'info' | 'warn' | 'danger' | 'secondary'> = {
      pendente: 'warn',
      confirmada: 'success',
      checkin: 'info',
      checkout: 'secondary',
      cancelada: 'danger',
    };
    return map[status];
  }
}
