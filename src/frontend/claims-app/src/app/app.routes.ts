import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/claims-list/claims-list.component').then(m => m.ClaimsListComponent),
    canActivate: [authGuard]
  },
  {
    path: 'claims/new',
    loadComponent: () => import('./features/fnol-intake/fnol-intake.component').then(m => m.FnolIntakeComponent),
    canActivate: [authGuard]
  },
  {
    path: 'claims/:id',
    loadComponent: () => import('./features/claim-detail/claim-detail.component').then(m => m.ClaimDetailComponent),
    canActivate: [authGuard]
  },
  {
    path: '**',
    redirectTo: ''
  }
];
