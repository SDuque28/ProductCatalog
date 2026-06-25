import { computed, Injectable, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { catchError, EMPTY, finalize, Subject, switchMap, tap } from 'rxjs';
import { ProductApiService } from '../../../core/services/product-api.service';
import { Product } from '../models/product.model';

@Injectable()
export class ProductListFacade {
  private static readonly defaultPage = 1;
  private static readonly defaultPageSize = 10;
  private static readonly genericErrorMessage =
    'Unable to load products at the moment. Please try again.';

  private readonly loadTrigger$ = new Subject<void>();
  private readonly searchTerm = signal('');
  private readonly onlyInStock = signal(false);

  public readonly products = signal<Product[]>([]);
  public readonly loading = signal(false);
  public readonly error = signal<string | null>(null);
  public readonly currentPage = signal(ProductListFacade.defaultPage);
  public readonly pageSize = signal(ProductListFacade.defaultPageSize);
  public readonly totalItems = signal(0);
  public readonly totalPages = computed(() => {
    const total = this.totalItems();
    const pageSize = this.pageSize();

    return total > 0 ? Math.ceil(total / pageSize) : 1;
  });
  public readonly hasProducts = computed(() => this.products().length > 0);
  public readonly hasPreviousPage = computed(() => this.currentPage() > ProductListFacade.defaultPage);
  public readonly hasNextPage = computed(() => this.currentPage() < this.totalPages());

  public constructor(private readonly productApiService: ProductApiService) {
    this.loadTrigger$
      .pipe(
        tap(() => {
          this.loading.set(true);
          this.error.set(null);
        }),
        switchMap(() =>
          this.productApiService
            .getProducts(
              this.getNombreFilter(),
              this.getStockFilter(),
              this.currentPage(),
              this.pageSize()
            )
            .pipe(
              tap((response) => {
                this.products.set(response.items);
                this.currentPage.set(response.page);
                this.pageSize.set(response.pageSize);
                this.totalItems.set(response.total);
              }),
              catchError(() => {
                this.products.set([]);
                this.totalItems.set(0);
                this.error.set(ProductListFacade.genericErrorMessage);

                return EMPTY;
              }),
              finalize(() => this.loading.set(false))
            )
        ),
        takeUntilDestroyed()
      )
      .subscribe();
  }

  public loadProducts(): void {
    this.loadTrigger$.next();
  }

  public searchProducts(nombre: string, conStock: boolean): void {
    this.searchTerm.set(nombre.trim());
    this.onlyInStock.set(conStock);
    this.currentPage.set(ProductListFacade.defaultPage);
    this.loadProducts();
  }

  public goToPreviousPage(): void {
    if (!this.hasPreviousPage()) {
      return;
    }

    this.currentPage.update((page) => page - 1);
    this.loadProducts();
  }

  public goToNextPage(): void {
    if (!this.hasNextPage()) {
      return;
    }

    this.currentPage.update((page) => page + 1);
    this.loadProducts();
  }

  private getNombreFilter(): string | undefined {
    const nombre = this.searchTerm();

    return nombre ? nombre : undefined;
  }

  private getStockFilter(): boolean | undefined {
    return this.onlyInStock() ? true : undefined;
  }
}
