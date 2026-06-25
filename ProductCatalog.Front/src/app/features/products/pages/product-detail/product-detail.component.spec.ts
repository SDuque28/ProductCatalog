import { signal, WritableSignal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { vi } from 'vitest';
import { Product } from '../../models/product.model';
import { ProductDetailFacade } from '../../services/product-detail.facade';
import { ProductDetailComponent } from './product-detail.component';

describe('ProductDetailComponent', () => {
  let facadeMock: ProductDetailFacadeMock;

  beforeEach(async () => {
    facadeMock = createProductDetailFacadeMock();

    TestBed.overrideComponent(ProductDetailComponent, {
      set: {
        providers: [
          {
            provide: ProductDetailFacade,
            useValue: facadeMock
          }
        ]
      }
    });

    await TestBed.configureTestingModule({
      imports: [ProductDetailComponent],
      providers: [
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: {
            paramMap: of(convertToParamMap({ id: '1' }))
          }
        }
      ]
    }).compileComponents();
  });

  it('should show loading state while product details are being fetched', () => {
    facadeMock.loading.set(true);

    const fixture = TestBed.createComponent(ProductDetailComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Loading product information...');
  });

  it('should show product not found state when the API returns 404', () => {
    facadeMock.notFound.set(true);

    const fixture = TestBed.createComponent(ProductDetailComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Product not found');
    expect(fixture.nativeElement.textContent).toContain('Back to Products');
  });

  it('should show generic error state when product loading fails', () => {
    facadeMock.error.set('Unable to load product information.');

    const fixture = TestBed.createComponent(ProductDetailComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Something went wrong');
    expect(fixture.nativeElement.textContent).toContain('Unable to load product information.');
    expect(fixture.nativeElement.textContent).toContain('Retry');
  });

  it('should render product details when data is available', () => {
    facadeMock.product.set({
      idProducto: 1,
      nombreProducto: 'Teclado Mecanico',
      descripcion: 'Teclado RGB Switch Azul',
      valor: 150000,
      stock: 12
    });

    const fixture = TestBed.createComponent(ProductDetailComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Teclado Mecanico');
    expect(fixture.nativeElement.textContent).toContain('Teclado RGB Switch Azul');
    expect(fixture.nativeElement.textContent).toContain('12 units');
  });

  function createProductDetailFacadeMock(): ProductDetailFacadeMock {
    return {
      product: signal<Product | null>(null),
      loading: signal(false),
      error: signal<string | null>(null),
      notFound: signal(false),
      hasProduct: signal(false),
      loadProductById: vi.fn(),
      retry: vi.fn()
    };
  }
});

type ProductDetailFacadeMock = {
  product: WritableSignal<Product | null>;
  loading: WritableSignal<boolean>;
  error: WritableSignal<string | null>;
  notFound: WritableSignal<boolean>;
  hasProduct: WritableSignal<boolean>;
  loadProductById: ReturnType<typeof vi.fn>;
  retry: ReturnType<typeof vi.fn>;
};
