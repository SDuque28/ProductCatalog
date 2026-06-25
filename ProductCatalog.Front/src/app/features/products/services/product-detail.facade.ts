import { HttpErrorResponse } from '@angular/common/http';
import { computed, Injectable, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { catchError, EMPTY, finalize, Subject, switchMap, tap } from 'rxjs';
import { ProductApiService } from '../../../core/services/product-api.service';
import { Product } from '../models/product.model';

@Injectable()
export class ProductDetailFacade {
  private static readonly invalidProductId = 0;
  private static readonly genericErrorMessage = 'Unable to load product information.';

  private readonly loadTrigger$ = new Subject<number>();
  private readonly selectedProductId = signal<number | null>(null);

  public readonly product = signal<Product | null>(null);
  public readonly loading = signal(false);
  public readonly error = signal<string | null>(null);
  public readonly notFound = signal(false);
  public readonly hasProduct = computed(() => this.product() !== null);

  public constructor(private readonly productApiService: ProductApiService) {
    this.loadTrigger$
      .pipe(
        tap((productId) => {
          this.selectedProductId.set(productId);
          this.loading.set(true);
          this.error.set(null);
          this.notFound.set(false);
        }),
        switchMap((productId) =>
          this.productApiService.getProductById(productId).pipe(
            tap((product) => {
              if (!product) {
                this.product.set(null);
                this.notFound.set(true);
                return;
              }

              this.product.set(product);
            }),
            catchError((error: HttpErrorResponse) => {
              this.product.set(null);

              if (error.status === 404) {
                this.notFound.set(true);
                return EMPTY;
              }

              this.error.set(ProductDetailFacade.genericErrorMessage);
              return EMPTY;
            }),
            finalize(() => this.loading.set(false))
          )
        ),
        takeUntilDestroyed()
      )
      .subscribe();
  }

  public loadProductById(id: number | null): void {
    if (id === null || id <= ProductDetailFacade.invalidProductId) {
      this.product.set(null);
      this.loading.set(false);
      this.error.set(null);
      this.notFound.set(true);
      this.selectedProductId.set(null);
      return;
    }

    this.loadTrigger$.next(id);
  }

  public retry(): void {
    const productId = this.selectedProductId();

    if (productId === null) {
      return;
    }

    this.loadTrigger$.next(productId);
  }
}
