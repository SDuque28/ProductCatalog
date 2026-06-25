import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'products'
  },
  {
    path: 'products',
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
