export interface InventoryItem {
  id: string;
  warehouse_id: string;
  warehouse_code: string;
  warehouse_name: string;
  product_id: string;
  product_code: string;
  product_name: string;
  category_id: string;
  category_name: string;
  brand?: string;
  unit_price: number;
  quantity: number;
  minimum_stock_level: number;
  is_low_stock: boolean;
  total_stock_value: number;
  last_updated_at: string;
}

export interface InventoryTransactionItem {
  id: string;
  warehouse_id: string;
  warehouse_code: string;
  warehouse_name: string;
  product_id: string;
  product_code: string;
  product_name: string;
  user_id: string;
  employee_code: string;
  user_full_name: string;
  transaction_type: number;
  transaction_type_name: string;
  quantity: number;
  transaction_date: string;
  reference_code?: string;
  note?: string;
}

export interface DashboardSummary {
  total_products: number;
  total_warehouses: number;
  total_suppliers: number;
  total_customers: number;
  total_stock_units: number;
  total_inventory_value: number;
  low_stock_alert_count: number;
  pending_purchase_orders: number;
  pending_outbound_orders: number;
  pending_stock_transfers: number;
  low_stock_items: InventoryItem[];
  recent_transactions: InventoryTransactionItem[];
}
