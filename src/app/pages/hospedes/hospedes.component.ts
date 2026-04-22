import { Component, inject, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Dialog } from 'primeng/dialog';
import { InputText } from 'primeng/inputtext';
import { InputMask } from 'primeng/inputmask';
import { Textarea } from 'primeng/textarea';
import { Button } from 'primeng/button';
import { IconField } from 'primeng/iconfield';
import { InputIcon } from 'primeng/inputicon';
import { Fluid } from 'primeng/fluid';
import { Tooltip } from 'primeng/tooltip';
import { HospedeService } from '../../core/services/hospede.service';
import { Hospede } from '../../core/models';

@Component({
  selector: 'app-hospedes',
  standalone: true,
  imports: [
    FormsModule,
    Dialog,
    InputText,
    InputMask,
    Textarea,
    Button,
    IconField,
    InputIcon,
    Fluid,
    Tooltip,
  ],
  templateUrl: './hospedes.component.html',
  styleUrl: './hospedes.component.scss',
})
export class HospedesComponent {
  private readonly hospedeService = inject(HospedeService);

  protected readonly hospedes = this.hospedeService.hospedes;
  protected dialogVisible = signal(false);
  protected editing = signal(false);
  protected searchValue = signal('');

  protected hospedeForm: Hospede = this.emptyHospede();

  protected filteredHospedes = computed(() => {
    const term = this.searchValue().toLowerCase();
    if (!term) return this.hospedes();
    return this.hospedes().filter(h =>
      h.nome.toLowerCase().includes(term) ||
      h.documento?.includes(term) ||
      h.email?.toLowerCase().includes(term) ||
      h.cidade?.toLowerCase().includes(term)
    );
  });

  protected getInitials(nome: string): string {
    return nome.split(' ').map(n => n[0]).slice(0, 2).join('').toUpperCase();
  }

  protected onSearch(event: Event): void {
    this.searchValue.set((event.target as HTMLInputElement).value);
  }

  protected openNew(): void {
    this.hospedeForm = this.emptyHospede();
    this.editing.set(false);
    this.dialogVisible.set(true);
  }

  protected editHospede(hospede: Hospede): void {
    this.hospedeForm = { ...hospede };
    this.editing.set(true);
    this.dialogVisible.set(true);
  }

  protected saveHospede(): void {
    if (!this.hospedeForm.nome.trim()) return;
    if (this.editing()) {
      this.hospedeService.update(this.hospedeForm);
    } else {
      this.hospedeService.add(this.hospedeForm);
    }
    this.dialogVisible.set(false);
  }

  private emptyHospede(): Hospede {
    return { id: 0, nome: '', documento: '', telefone: '', email: '', cidade: '' };
  }
}
