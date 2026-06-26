import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { getProductStockStatus } from '../../models/product-stock-status';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  templateUrl: './status-badge.component.html',
  styleUrl: './status-badge.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class StatusBadgeComponent {
  public readonly stock = input.required<number>();
  protected readonly status = computed(() => getProductStockStatus(this.stock()));
}
