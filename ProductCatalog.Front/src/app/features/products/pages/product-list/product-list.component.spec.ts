import { signal, WritableSignal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { vi } from 'vitest';
import { Product } from '../../models/product.model';
import { ProductListComponent } from './product-list.component';
import { ProductListFacade } from '../../services/product-list.facade';

describe('ProductListComponent', () => {
  let facadeMock: ProductListFacadeMock;

  beforeEach(async () => {
    facadeMock = createProductListFacadeMock();

    TestBed.overrideComponent(ProductListComponent, {
      set: {
        providers: [
          {
            provide: ProductListFacade,
            useValue: facadeMock
          }
        ]
      }
    });

    await TestBed.configureTestingModule({
      imports: [ProductListComponent],
      providers: [provideRouter([])]
    }).compileComponents();
  });

  it('should show loading state while products are being fetched', () => {
    facadeMock.loading.set(true);

    const fixture = TestBed.createComponent(ProductListComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Loading products...');
  });

  it('should show error state when product loading fails', () => {
    facadeMock.error.set('Unable to load products. Please try again later.');

    const fixture = TestBed.createComponent(ProductListComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Something went wrong');
    expect(fixture.nativeElement.textContent).toContain('Unable to load products. Please try again later.');
  });

  it('should show empty state when no products are returned', () => {
    const fixture = TestBed.createComponent(ProductListComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('No products found');
    expect(fixture.nativeElement.textContent).toContain('Try changing the search filters.');
  });

  it('should render the table when products are available', () => {
    facadeMock.products.set([
      {
        idProducto: 1,
        nombreProducto: 'Mouse',
        descripcion: 'Mouse ergonomico',
        valor: 85000,
        stock: 10
      }
    ]);
    facadeMock.hasProducts.set(true);

    const fixture = TestBed.createComponent(ProductListComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('table')).toBeTruthy();
    expect(fixture.nativeElement.textContent).toContain('Mouse');
  });

  function createProductListFacadeMock(): ProductListFacadeMock {
    const products = signal<Product[]>([]);
    const loading = signal(false);
    const error = signal<string | null>(null);
    const currentPage = signal(1);
    const pageSize = signal(10);
    const totalItems = signal(0);
    const totalPages = signal(1);
    const hasProducts = signal(false);
    const hasPreviousPage = signal(false);
    const hasNextPage = signal(false);

    return {
      products,
      loading,
      error,
      currentPage,
      pageSize,
      totalItems,
      totalPages,
      hasProducts,
      hasPreviousPage,
      hasNextPage,
      loadProducts: vi.fn(),
      searchProducts: vi.fn(),
      goToPreviousPage: vi.fn(),
      goToNextPage: vi.fn()
    };
  }
});

type ProductListFacadeMock = {
  products: WritableSignal<Product[]>;
  loading: WritableSignal<boolean>;
  error: WritableSignal<string | null>;
  currentPage: WritableSignal<number>;
  pageSize: WritableSignal<number>;
  totalItems: WritableSignal<number>;
  totalPages: WritableSignal<number>;
  hasProducts: WritableSignal<boolean>;
  hasPreviousPage: WritableSignal<boolean>;
  hasNextPage: WritableSignal<boolean>;
  loadProducts: ReturnType<typeof vi.fn>;
  searchProducts: ReturnType<typeof vi.fn>;
  goToPreviousPage: ReturnType<typeof vi.fn>;
  goToNextPage: ReturnType<typeof vi.fn>;
};
