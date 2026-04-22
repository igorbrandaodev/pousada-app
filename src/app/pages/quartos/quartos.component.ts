import { Component, inject, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputText } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { DropdownModule } from 'primeng/dropdown';
import { SelectButtonModule } from 'primeng/selectbutton';
import { QuartoService } from '../../core/services/quarto.service';
import { ReservaService } from '../../core/services/reserva.service';
import { HospedeService } from '../../core/services/hospede.service';
import { Quarto } from '../../core/models';

@Component({
  selector: 'app-quartos',
  standalone: true,
  imports: [
    FormsModule,
    ButtonModule,
    DialogModule,
    InputText,
    InputNumberModule,
    DropdownModule,
    SelectButtonModule,
  ],
  templateUrl: './quartos.component.html',
  styleUrl: './quartos.component.scss'
})
export class QuartosComponent {
  private readonly quartoService = inject(QuartoService);
  private readonly reservaService = inject(ReservaService);
  private readonly hospedeService = inject(HospedeService);

  protected statusFilter = signal<string>('todos');
  protected dialogVisible = signal(false);
  protected isNewQuarto = signal(false);
  protected editingQuarto = signal<Quarto | null>(null);

  protected readonly statusOptions = [
    { label: 'Todos', value: 'todos' },
    { label: 'Disponivel', value: 'disponivel' },
    { label: 'Ocupado', value: 'ocupado' },
    { label: 'Limpeza', value: 'limpeza' },
    { label: 'Manutencao', value: 'manutencao' },
  ];

  protected readonly statusDropdownOptions = [
    { label: 'Disponivel', value: 'disponivel' },
    { label: 'Ocupado', value: 'ocupado' },
    { label: 'Limpeza', value: 'limpeza' },
    { label: 'Manutencao', value: 'manutencao' },
  ];

  protected readonly tipoOptions = [
    { label: 'Standard', value: 'standard' },
    { label: 'Luxo', value: 'luxo' },
    { label: 'Suite', value: 'suite' },
  ];

  protected quartosFiltrados = computed(() => {
    const filter = this.statusFilter();
    const all = this.quartoService.quartos();
    if (filter === 'todos') return all;
    return all.filter(q => q.status === filter);
  });

  protected onFilterChange(value: string): void {
    this.statusFilter.set(value ?? 'todos');
  }

  protected getStatusLabel(status: Quarto['status']): string {
    const map: Record<Quarto['status'], string> = {
      disponivel: 'Disponivel',
      ocupado: 'Ocupado',
      limpeza: 'Limpeza',
      manutencao: 'Manutencao',
    };
    return map[status];
  }

  protected getHospedeAtual(quarto: Quarto): string | null {
    const reserva = this.reservaService.reservas().find(r => r.idQuarto === quarto.id && r.status === 'checkin');
    if (!reserva) return null;
    return this.hospedeService.getById(reserva.idHospede)?.nome ?? null;
  }

  protected openNew(): void {
    this.editingQuarto.set({
      id: 0,
      numero: '',
      tipo: 'standard',
      capacidade: 2,
      precoDiaria: 0,
      status: 'disponivel',
      andar: 1,
      amenidades: [],
    });
    this.isNewQuarto.set(true);
    this.dialogVisible.set(true);
  }

  protected openDetails(quarto: Quarto): void {
    this.editingQuarto.set({ ...quarto });
    this.isNewQuarto.set(false);
    this.dialogVisible.set(true);
  }

  protected updateField(field: keyof Quarto, value: any): void {
    const current = this.editingQuarto();
    if (!current) return;
    this.editingQuarto.set({ ...current, [field]: value });
  }

  protected save(): void {
    const q = this.editingQuarto();
    if (!q) return;
    if (this.isNewQuarto()) {
      this.quartoService.add(q);
    } else {
      this.quartoService.update(q);
    }
    this.dialogVisible.set(false);
  }

  protected cancel(): void {
    this.dialogVisible.set(false);
  }
}
