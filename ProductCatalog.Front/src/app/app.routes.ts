import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { guestGuard } from './core/guards/guest.guard';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'products'
  },
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./features/auth/pages/login/login.component').then((module) => module.LoginComponent)
  },
  {
    path: 'products',
    canActivate: [authGuard],
    loadChildren: () =>
      import('./features/products/routes/products.routes').then((module) => module.PRODUCT_ROUTES)
  },
  {
    path: '**',
    loadComponent: () =>
      import('./layout/components/page-not-found/page-not-found.component').then(
        (module) => module.PageNotFoundComponent
      )
  }
];
