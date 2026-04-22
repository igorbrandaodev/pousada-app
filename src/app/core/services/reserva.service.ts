import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Reserva } from '../models';
import { environment } from '../../environments/environment';
import { QuartoService } from './quarto.service';
import { HospedeService } from './hospede.service';

@Injectable({ providedIn: 'root' })
export class ReservaService {
  private readonly http = inject(HttpClient);
  private readonly quartoService = inject(QuartoService);
  private readonly hospedeService = inject(HospedeService);
  private readonly url = `${environment.apiUrl}/Reservas`;

  private _reservas = signal<Reserva[]>([]);
  public reservas = this._reservas.asReadonly();

  public reservasHoje = computed(() => {
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const tomorrow = new Date(today);
    tomorrow.setDate(tomorrow.getDate() + 1);

    return this._reservas()
      .filter(r => {
        const checkin = new Date(r.dataCheckin);
        const checkout = new Date(r.dataCheckout);
        checkin.setHours(0, 0, 0, 0);
        checkout.setHours(0, 0, 0, 0);
        return (checkin.getTime() === today.getTime() || checkout.getTime() === today.getTime());
      })
      .map(r => this.populateReserva(r));
  });

  public reservasAtivas = computed(() =>
    this._reservas()
      .filter(r => r.status === 'checkin')
      .map(r => this.populateReserva(r))
  );

  constructor() {
    this.loadAll();
  }

  loadAll(): void {
    this.http.get<any[]>(this.url).subscribe({
      next: (data) => this._reservas.set(data.map(r => this.mapReserva(r))),
      error: (err) => console.error('Failed to load reservas:', err)
    });
  }

  getById(id: number): Reserva | undefined {
    const reserva = this._reservas().find(r => r.id === id);
    return reserva ? this.populateReserva(reserva) : undefined;
  }

  add(reserva: Partial<Reserva>): void {
    this.http.post(this.url, {
      idHospede: reserva.idHospede,
      idQuarto: reserva.idQuarto,
      dataCheckin: reserva.dataCheckin,
      dataCheckout: reserva.dataCheckout,
      observacoes: reserva.observacoes
    }).subscribe({ next: () => this.loadAll() });
  }

  update(reserva: Reserva): void {
    this.http.put(`${this.url}/${reserva.id}`, {
      idHospede: reserva.idHospede,
      idQuarto: reserva.idQuarto,
      dataCheckin: reserva.dataCheckin,
      dataCheckout: reserva.dataCheckout,
      observacoes: reserva.observacoes
    }).subscribe({ next: () => this.loadAll() });
  }

  updateStatus(id: number, status: Reserva['status']): void {
    const capitalStatus = status.charAt(0).toUpperCase() + status.slice(1);
    this.http.patch(`${this.url}/${id}/status`, { status: capitalStatus })
      .subscribe({
        next: () => {
          this.loadAll();
          this.quartoService.loadAll();
        }
      });
  }

  getByQuarto(idQuarto: number): Reserva[] {
    return this._reservas()
      .filter(r => r.idQuarto === idQuarto)
      .map(r => this.populateReserva(r));
  }

  getByHospede(idHospede: number): Reserva[] {
    return this._reservas()
      .filter(r => r.idHospede === idHospede)
      .map(r => this.populateReserva(r));
  }

  getReservasPorData(date: Date): Reserva[] {
    const target = new Date(date);
    target.setHours(0, 0, 0, 0);

    return this._reservas()
      .filter(r => {
        const checkin = new Date(r.dataCheckin);
        const checkout = new Date(r.dataCheckout);
        checkin.setHours(0, 0, 0, 0);
        checkout.setHours(0, 0, 0, 0);
        return checkin <= target && checkout >= target && r.status !== 'cancelada';
      })
      .map(r => this.populateReserva(r));
  }

  private mapReserva(r: any): Reserva {
    return {
      ...r,
      status: r.status?.toLowerCase() || 'pendente',
      dataCheckin: new Date(r.dataCheckin),
      dataCheckout: new Date(r.dataCheckout)
    };
  }

  private populateReserva(reserva: Reserva): Reserva {
    return {
      ...reserva,
      hospede: this.hospedeService.getById(reserva.idHospede),
      quarto: this.quartoService.getById(reserva.idQuarto),
    };
  }
}
