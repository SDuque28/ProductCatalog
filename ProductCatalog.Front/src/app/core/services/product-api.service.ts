import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../constants/api.constants';
import { PagedResponse } from '../models/paged-response.model';
import { Product } from '../../features/products/models/product.model';

@Injectable({
  providedIn: 'root'
})
export class ProductApiService {
  private readonly httpClient = inject(HttpClient);
  private readonly productsUrl = `${API_BASE_URL}/products`;

  public getProducts(
    nombre?: string,
    conStock?: boolean,
    page: number = 1,
    pageSize: number = 10
  ): Observable<PagedResponse<Product>> {
    let params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);

    if (nombre?.trim()) {
      params = params.set('name', nombre.trim());
    }

    if (conStock !== undefined) {
      params = params.set('inStock', conStock);
    }

    return this.httpClient.get<PagedResponse<Product>>(this.productsUrl, { params });
  }

  public getProductById(id: number): Observable<Product> {
    return this.httpClient.get<Product>(`${this.productsUrl}/${id}`);
  }
}
