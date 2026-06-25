import { CommonModule, CurrencyPipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { ReactiveFormsModule, FormControl, FormGroup } from '@angular/forms';
import { Router } from '@angular/router';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';
import { ErrorStateComponent } from '../../../../shared/components/error-state/error-state.component';
import { LoadingSpinnerComponent } from '../../../../shared/components/loading-spinner/loading-spinner.component';
import { Product } from '../../models/product.model';
import { ProductListFacade } from '../../services/product-list.facade';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    CurrencyPipe,
    LoadingSpinnerComponent,
    ErrorStateComponent,
    EmptyStateComponent
  ],
  templateUrl: './product-list.component.html',
  styleUrl: './product-list.component.scss',
  providers: [ProductListFacade]
})
export class ProductListComponent implements OnInit {
  public readonly facade = inject(ProductListFacade);
  private readonly router = inject(Router);
  public readonly filtersForm = new FormGroup({
    nombre: new FormControl('', { nonNullable: true }),
    conStock: new FormControl(false, { nonNullable: true })
  });

  public ngOnInit(): void {
    this.facade.loadProducts();
  }

  public onSearch(): void {
    const { nombre, conStock } = this.filtersForm.getRawValue();

    this.facade.searchProducts(nombre, conStock);
  }

  public onPreviousPage(): void {
    this.facade.goToPreviousPage();
  }

  public onNextPage(): void {
    this.facade.goToNextPage();
  }

  public viewProductDetail(product: Product): void {
    void this.router.navigate(['/products', product.idProducto]);
  }

  public trackByProductId(_index: number, product: Product): number {
    return product.idProducto;
  }
}
