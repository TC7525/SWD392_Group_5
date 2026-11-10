export interface CategoryItem {
  id: string;
  category_code: string;
  category_name: string;
  description?: string;
  status: number;
  status_name: string;
  product_count: number;
}

export interface WarehouseItem {
  id: string;
  warehouse_code: string;
  warehouse_name: string;
  address?: string;
  status: number;
  status_name: string;
  total_products: number;
  total_quantity: number;
  assigned_staff_count: number;
  created_on?: string;
}

export interface ProductItem {
  id: string;
  product_code: string;
  product_name: string;
  category_id: string;
  category_name: string;
  brand?: string;
  description?: string;
  unit_price: number;
  minimum_stock_level: number;
  current_warehouse_stock: number;
  total_stock: number;
  is_low_stock: boolean;
  status: number;
  status_name: string;
}

export interface SupplierItem {
  id: string;
  supplier_code: string;
  supplier_name: string;
  contact_person?: string;
  phone?: string;
  email?: string;
  address?: string;
  status: number;
  status_name: string;
  total_purchase_orders: number;
}

export interface CustomerItem {
  id: string;
  customer_code: string;
  customer_name: string;
  phone?: string;
  email?: string;
  address?: string;
  status: number;
  status_name: string;
  total_orders: number;
}

export interface PermissionItem {
  id: string;
  permission_code: string;
  permission_name: string;
  module: string;
  action: string;
  description?: string;
}

export interface RoleItem {
  id: string;
  role_code: string;
  role_name: string;
  description?: string;
  status?: number;
  status_name?: string;
  user_count?: number;
  permissions?: PermissionItem[];
  permission_count?: number;
}

export interface RoleDetailItem {
  id: string;
  role_code: string;
  role_name: string;
  description?: string;
  status: number;
  status_name?: string;
  permission_ids: string[];
  permissions: PermissionItem[];
}

export interface AccountItem {
  id: string;
  employee_code: string;
  full_name: string;
  email: string;
  phone?: string;
  status: number;
  status_name: string;
  role_id: string;
  role_code: string;
  role_name: string;
  created_on?: string;
  modified_on?: string;
  assigned_warehouses: {
    warehouse_id: string;
    warehouse_code: string;
    warehouse_name: string;
  }[];
}

export interface AccessLogItem {
  id: string;
  user_id?: string;
  employee_code?: string;
  full_name?: string;
  module: string;
  action: string;
  entity_type?: string;
  entity_id?: string;
  description?: string;
  ip_address?: string;
  created_on?: string;
}
