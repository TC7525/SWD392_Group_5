import { type PagedResult } from '../types/auth.types';
import {
  type DashboardSummary,
  type InventoryItem,
  type InventoryTransactionItem,
} from '../types/inventory.types';
import { httpRequest } from './httpClient';

export const inventoryService = {
  getDashboardSummary: (warehouseId?: string) =>
    httpRequest<DashboardSummary>(
      `api/inventory/dashboard-summary${
        warehouseId ? `?warehouse_id=${warehouseId}` : ''
      }`
    ),

  searchInventory: (filter: {
    warehouse_id?: string;
    keyword?: string;
    category_id?: string;
    low_stock_only?: boolean;
    page_index?: number;
    page_size?: number;
  }) =>
    httpRequest<PagedResult<InventoryItem>>('api/inventory/search', {
      method: 'POST',
      body: JSON.stringify({
        page_index: 1,
        page_size: 50,
        ...filter,
      }),
    }),

  adjustInventory: (payload: {
    warehouse_id: string;
    product_id: string;
    new_quantity: number;
    note: string;
  }) =>
    httpRequest<boolean>('api/inventory/adjust', {
      method: 'POST',
      body: JSON.stringify(payload),
    }),

  searchTransactions: (filter: {
    warehouse_id?: string;
    product_id?: string;
    page_index?: number;
    page_size?: number;
  }) =>
    httpRequest<PagedResult<InventoryTransactionItem>>(
      'api/inventory/transactions/search',
      {
        method: 'POST',
        body: JSON.stringify({
          page_index: 1,
          page_size: 50,
          ...filter,
        }),
      }
    ),
};
