import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { vi } from 'vitest';
import { PagedResponse } from '../../../core/models/paged-response.model';
import { ProductApiService } from '../../../core/services/product-api.service';
import { Product } from '../models/product.model';
import { ProductListFacade } from './product-list.facade';

describe('ProductListFacade', () => {
  let facade: ProductListFacade;
  let productApiServiceSpy: Pick<ProductApiService, 'getProducts'>;

  beforeEach(() => {
    productApiServiceSpy = {
      getProducts: vi.fn()
    };

    TestBed.configureTestingModule({
      providers: [
        ProductListFacade,
        {
          provide: ProductApiService,
          useValue: productApiServiceSpy
        }
      ]
    });

    facade = TestBed.inject(ProductListFacade);
  });

  it('loadProducts_WhenResponseHasMultiplePages_ShouldCalculatePaginationState', () => {
    const response = createPagedResponse({
      page: 1,
      pageSize: 10,
      total: 25
    });

    vi.mocked(productApiServiceSpy.getProducts).mockReturnValue(of(response));

    facade.loadProducts();

    expect(facade.products()).toEqual(response.items);
    expect(facade.totalItems()).toBe(25);
    expect(facade.totalPages()).toBe(3);
    expect(facade.hasPreviousPage()).toBe(false);
    expect(facade.hasNextPage()).toBe(true);
  });

  it('goToNextPage_WhenAnotherPageExists_ShouldAdvanceAndReloadProducts', () => {
    vi.mocked(productApiServiceSpy.getProducts).mockReturnValueOnce(
      of(
        createPagedResponse({
          page: 1,
          pageSize: 10,
          total: 25
        })
      )
    );
    vi.mocked(productApiServiceSpy.getProducts).mockReturnValueOnce(
      of(
        createPagedResponse({
          page: 2,
          pageSize: 10,
          total: 25
        })
      )
    );

    facade.loadProducts();
    facade.goToNextPage();

    expect(facade.currentPage()).toBe(2);
    expect(productApiServiceSpy.getProducts).toHaveBeenCalledWith(2, 10, undefined, undefined);
  });

  it('goToPreviousPage_WhenCurrentPageIsFirstPage_ShouldNotReloadProducts', () => {
    vi.mocked(productApiServiceSpy.getProducts).mockReturnValue(
      of(
        createPagedResponse({
          page: 1,
          pageSize: 10,
          total: 25
        })
      )
    );

    facade.loadProducts();
    facade.goToPreviousPage();

    expect(facade.currentPage()).toBe(1);
    expect(productApiServiceSpy.getProducts).toHaveBeenCalledTimes(1);
    expect(facade.hasPreviousPage()).toBe(false);
  });

  it('goToNextPage_WhenCurrentPageIsLastPage_ShouldNotReloadProducts', () => {
    vi.mocked(productApiServiceSpy.getProducts).mockReturnValueOnce(
      of(
        createPagedResponse({
          page: 1,
          pageSize: 10,
          total: 15
        })
      )
    );
    vi.mocked(productApiServiceSpy.getProducts).mockReturnValueOnce(
      of(
        createPagedResponse({
          page: 2,
          pageSize: 10,
          total: 15
        })
      )
    );

    facade.loadProducts();
    facade.goToNextPage();
    facade.goToNextPage();

    expect(facade.currentPage()).toBe(2);
    expect(productApiServiceSpy.getProducts).toHaveBeenCalledTimes(2);
    expect(facade.hasNextPage()).toBe(false);
  });

  function createPagedResponse({
    page,
    pageSize,
    total
  }: {
    page: number;
    pageSize: number;
    total: number;
  }): PagedResponse<Product> {
    return {
      page,
      pageSize,
      total,
      items: Array.from({ length: Math.min(pageSize, total) }, (_, index) => ({
        idProducto: (page - 1) * pageSize + index + 1,
        nombreProducto: `Producto ${(page - 1) * pageSize + index + 1}`,
        descripcion: `Descripcion ${(page - 1) * pageSize + index + 1}`,
        valor: 1000,
        stock: 5
      }))
    };
  }
});
