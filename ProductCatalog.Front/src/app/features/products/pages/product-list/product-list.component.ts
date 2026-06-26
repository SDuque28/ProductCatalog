import { CommonModule, CurrencyPipe } from '@angular/common';
import { Component, computed, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ReactiveFormsModule, FormControl, FormGroup } from '@angular/forms';
import { Router } from '@angular/router';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';
import { ErrorStateComponent } from '../../../../shared/components/error-state/error-state.component';
import { LoadingSpinnerComponent } from '../../../../shared/components/loading-spinner/loading-spinner.component';
import { StatusBadgeComponent } from '../../components/status-badge/status-badge.component';
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
    EmptyStateComponent,
    StatusBadgeComponent
  ],
  templateUrl: './product-list.component.html',
  styleUrl: './product-list.component.scss',
  providers: [ProductListFacade]
})
export class ProductListComponent implements OnInit {
  public readonly facade = inject(ProductListFacade);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  public readonly filtersForm = new FormGroup({
    nombre: new FormControl('', { nonNullable: true }),
    conStock: new FormControl(false, { nonNullable: true })
  });
  public readonly totalItemsSummary = computed(() => {
    const totalItems = this.facade.totalItems();

    if (totalItems === 0) {
      return 'Search and review products from the current inventory feed.';
    }

    return `${totalItems} product${totalItems === 1 ? '' : 's'} available in the catalog.`;
  });
  public readonly currentRangeSummary = computed(() => {
    const totalItems = this.facade.totalItems();

    if (totalItems === 0) {
      return 'No results to display';
    }

    const page = this.facade.currentPage();
    const pageSize = this.facade.pageSize();
    const startItem = (page - 1) * pageSize + 1;
    const endItem = Math.min(page * pageSize, totalItems);

    return `Showing ${startItem}-${endItem} of ${totalItems}`;
  });
  public readonly pageNumbers = computed(() =>
    Array.from({ length: this.facade.totalPages() }, (_value, index) => index + 1)
  );

  public ngOnInit(): void {
    this.filtersForm.controls.conStock.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((conStock) => {
        this.facade.searchProducts(this.filtersForm.controls.nombre.getRawValue(), conStock);
      });

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

  public onPageSelect(page: number): void {
    this.facade.goToPage(page);
  }

  public onRetry(): void {
    this.facade.loadProducts();
  }

  public viewProductDetail(product: Product): void {
    void this.router.navigate(['/products', product.idProducto]);
  }

  public trackByProductId(_index: number, product: Product): number {
    return product.idProducto;
  }
}
