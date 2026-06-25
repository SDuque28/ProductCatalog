import { CommonModule, CurrencyPipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, ParamMap, RouterLink } from '@angular/router';
import { map } from 'rxjs';
import { ErrorMessageComponent } from '../../../../shared/components/error-message/error-message.component';
import { LoadingComponent } from '../../../../shared/components/loading/loading.component';
import { ProductDetailFacade } from '../../services/product-detail.facade';

@Component({
  selector: 'app-product-detail',
  imports: [CommonModule, CurrencyPipe, RouterLink, LoadingComponent, ErrorMessageComponent],
  standalone: true,
  templateUrl: './product-detail.component.html',
  styleUrl: './product-detail.component.scss',
  providers: [ProductDetailFacade]
})
export class ProductDetailComponent implements OnInit {
  public readonly facade = inject(ProductDetailFacade);
  private readonly activatedRoute = inject(ActivatedRoute);

  public ngOnInit(): void {
    this.activatedRoute.paramMap
      .pipe(
        map((paramMap: ParamMap) => this.parseProductId(paramMap)),
        takeUntilDestroyed()
      )
      .subscribe((productId) => this.facade.loadProductById(productId));
  }

  public retry(): void {
    this.facade.retry();
  }

  private parseProductId(paramMap: ParamMap): number | null {
    const id = Number(paramMap.get('id'));

    return Number.isInteger(id) ? id : null;
  }
}
