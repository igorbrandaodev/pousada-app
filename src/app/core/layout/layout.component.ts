import { Component, computed, signal } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { DrawerModule } from 'primeng/drawer';
import { AvatarModule } from 'primeng/avatar';
import { RippleModule } from 'primeng/ripple';
import { AuthService } from '../services/auth.service';

interface MenuItem {
  label: string;
  icon: string;
  matIcon?: string;
  route: string;
}

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    ButtonModule,
    DrawerModule,
    AvatarModule,
    RippleModule
  ],
  templateUrl: './layout.component.html',
  styleUrl: './layout.component.scss'
})
export class LayoutComponent {
  protected readonly sidebarVisible = signal(false);
  protected readonly userName = computed(() => this.authService.currentUser()?.nome ?? 'Admin');
  protected readonly userInitial = computed(() => this.userName().charAt(0).toUpperCase());

  protected readonly menuItems: MenuItem[] = [
    { label: 'Dashboard', icon: '', matIcon: 'dashboard', route: '/dashboard' },
    { label: 'Reservas', icon: '', matIcon: 'event_note', route: '/reservas' },
    { label: 'Agenda', icon: '', matIcon: 'calendar_month', route: '/agenda' },
    { label: 'Restaurante', icon: '', matIcon: 'restaurant', route: '/restaurante' },
    { label: 'Financeiro', icon: '', matIcon: 'payments', route: '/financeiro' },
    { label: 'Quartos', icon: '', matIcon: 'bed', route: '/quartos' },
    { label: 'Hospedes', icon: '', matIcon: 'people', route: '/hospedes' },
    { label: 'Cardapio', icon: '', matIcon: 'menu_book', route: '/cardapio' },
  ];

  constructor(private readonly authService: AuthService) {}

  protected toggleSidebar(): void {
    this.sidebarVisible.update(v => !v);
  }

  protected closeSidebar(): void {
    this.sidebarVisible.set(false);
  }

  protected logout(): void {
    this.authService.logout();
  }
}
