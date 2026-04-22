import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./pages/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: '',
    loadComponent: () => import('./core/layout/layout.component').then(m => m.LayoutComponent),
    canActivate: [authGuard],
    children: [
      { path: 'dashboard', loadComponent: () => import('./pages/dashboard/dashboard.component').then(m => m.DashboardComponent) },
      { path: 'agenda', loadComponent: () => import('./pages/agenda/agenda.component').then(m => m.AgendaComponent) },
      { path: 'quartos', loadComponent: () => import('./pages/quartos/quartos.component').then(m => m.QuartosComponent) },
      { path: 'reservas', loadComponent: () => import('./pages/reservas/reservas.component').then(m => m.ReservasComponent) },
      { path: 'hospedes', loadComponent: () => import('./pages/hospedes/hospedes.component').then(m => m.HospedesComponent) },
      { path: 'restaurante', loadComponent: () => import('./pages/restaurante/restaurante.component').then(m => m.RestauranteComponent) },
      { path: 'cardapio', loadComponent: () => import('./pages/cardapio/cardapio.component').then(m => m.CardapioComponent) },
      { path: 'financeiro', loadComponent: () => import('./pages/financeiro/financeiro.component').then(m => m.FinanceiroComponent) },
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
    ]
  },
  { path: '**', redirectTo: 'dashboard' }
];
