import { type PagedResult } from '../types/auth.types';
import {
  type OutboundOrderItem,
  type PurchaseOrderDetail,
  type PurchaseOrderItem,
  type StockTransferItem,
} from '../types/order.types';
import { httpRequest } from './httpClient';

export const purchaseOrderService = {
  search: (warehouse_id?: string) =>
    httpRequest<PagedResult<PurchaseOrderItem>>('api/purchase-order/search', {
      method: 'POST',
      body: JSON.stringify({ warehouse_id, page_index: 1, page_size: 50 }),
    }),

  getById: (id: string) =>
    httpRequest<PurchaseOrderDetail>(`api/purchase-order/${id}`),

  create: (payload: {
    supplier_id: string;
    warehouse_id: string;
    note?: string;
    items: {
      product_id: string;
      ordered_quantity: number;
      unit_price: number;
    }[];
  }) =>
    httpRequest<string>('api/purchase-order/create', {
      method: 'POST',
      body: JSON.stringify(payload),
    }),

  approve: (id: string) =>
    httpRequest<boolean>('api/purchase-order/approve', {
      method: 'POST',
      body: JSON.stringify({ id }),
    }),

  reject: (id: string, rejection_reason: string) =>
    httpRequest<boolean>('api/purchase-order/reject', {
      method: 'POST',
      body: JSON.stringify({ id, rejection_reason }),
    }),

  receiveGoods: (
    id: string,
    items: { purchase_order_item_id: string; receive_quantity: number }[],
    note?: string
  ) =>
    httpRequest<boolean>('api/purchase-order/receive-goods', {
      method: 'POST',
      body: JSON.stringify({ id, items, note }),
    }),
};

export const outboundOrderService = {
  search: (warehouse_id?: string) =>
    httpRequest<PagedResult<OutboundOrderItem>>('api/outbound-order/search', {
      method: 'POST',
      body: JSON.stringify({ warehouse_id, page_index: 1, page_size: 50 }),
    }),

  create: (payload: {
    customer_id: string;
    warehouse_id: string;
    note?: string;
    items: { product_id: string; quantity: number; unit_price?: number }[];
  }) =>
    httpRequest<string>('api/outbound-order/create', {
      method: 'POST',
      body: JSON.stringify(payload),
    }),

  approve: (id: string) =>
    httpRequest<boolean>('api/outbound-order/approve', {
      method: 'POST',
      body: JSON.stringify({ id }),
    }),

  reject: (id: string, rejection_reason: string) =>
    httpRequest<boolean>('api/outbound-order/reject', {
      method: 'POST',
      body: JSON.stringify({ id, rejection_reason }),
    }),

  issueGoods: (id: string, note?: string) =>
    httpRequest<boolean>('api/outbound-order/issue-goods', {
      method: 'POST',
      body: JSON.stringify({ id, note }),
    }),
};

export const stockTransferService = {
  search: () =>
    httpRequest<PagedResult<StockTransferItem>>('api/stock-transfer/search', {
      method: 'POST',
      body: JSON.stringify({ page_index: 1, page_size: 50 }),
    }),

  create: (payload: {
    source_warehouse_id: string;
    destination_warehouse_id: string;
    note?: string;
    items: { product_id: string; quantity: number; note?: string }[];
  }) =>
    httpRequest<string>('api/stock-transfer/create', {
      method: 'POST',
      body: JSON.stringify(payload),
    }),

  approve: (id: string) =>
    httpRequest<boolean>('api/stock-transfer/approve', {
      method: 'POST',
      body: JSON.stringify({ id }),
    }),

  dispatch: (id: string, note?: string) =>
    httpRequest<boolean>('api/stock-transfer/dispatch', {
      method: 'POST',
      body: JSON.stringify({ id, note }),
    }),

  receive: (id: string, note?: string) =>
    httpRequest<boolean>('api/stock-transfer/receive', {
      method: 'POST',
      body: JSON.stringify({ id, note }),
    }),
};
