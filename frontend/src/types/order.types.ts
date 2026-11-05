export interface PurchaseOrderItem {
  id: string;
  purchase_order_code: string;
  supplier_id: string;
  supplier_code: string;
  supplier_name: string;
  warehouse_id: string;
  warehouse_code: string;
  warehouse_name: string;
  order_date: string;
  total_ordered_quantity: number;
  total_received_quantity: number;
  total_amount: number;
  status: number;
  status_name: string;
  rejection_reason?: string;
  note?: string;
}

export interface PurchaseOrderDetail extends PurchaseOrderItem {
  items: {
    id: string;
    product_id: string;
    product_code: string;
    product_name: string;
    ordered_quantity: number;
    received_quantity: number;
    remaining_quantity: number;
    unit_price: number;
  }[];
}

export interface OutboundOrderItem {
  id: string;
  order_code: string;
  customer_id: string;
  customer_code: string;
  customer_name: string;
  warehouse_id: string;
  warehouse_code: string;
  warehouse_name: string;
  order_date: string;
  total_quantity: number;
  total_amount: number;
  status: number;
  status_name: string;
  rejection_reason?: string;
  note?: string;
}

export interface StockTransferItem {
  id: string;
  stock_transfer_code: string;
  source_warehouse_id: string;
  source_warehouse_code: string;
  source_warehouse_name: string;
  destination_warehouse_id: string;
  destination_warehouse_code: string;
  destination_warehouse_name: string;
  transfer_date: string;
  total_quantity: number;
  status: number;
  status_name: string;
  rejection_reason?: string;
  cancel_reason?: string;
  note?: string;
}
