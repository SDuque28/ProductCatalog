export type ProductStockStatusTone = 'available' | 'low' | 'out';

export interface ProductStockStatus {
  label: string;
  tone: ProductStockStatusTone;
}

export function getProductStockStatus(stock: number): ProductStockStatus {
  if (stock === 0) {
    return {
      label: 'Out of Stock',
      tone: 'out'
    };
  }

  if (stock <= 10) {
    return {
      label: 'Low Stock',
      tone: 'low'
    };
  }

  return {
    label: 'Available',
    tone: 'available'
  };
}
