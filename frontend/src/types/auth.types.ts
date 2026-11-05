export interface ApiResponse<T> {
  result?: T;
  Result?: T;
  errors?: string[];
  Errors?: string[];
  message?: string;
  Message?: string;
}

export interface PagedResult<T> {
  items: T[];
  Items: T[];
  total_items: number;
  page_index: number;
  page_size: number;
}

export interface AssignedWarehouse {
  warehouse_id: string;
  warehouse_code: string;
  warehouse_name: string;
  address?: string;
}

export interface UserProfile {
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
  permissions: string[];
  current_warehouse_id: string;
  current_warehouse_code: string;
  current_warehouse_name: string;
  assigned_warehouses: AssignedWarehouse[];
}

export interface LoginResponse {
  access_token: string;
  expires_at: string;
  user: UserProfile;
}

export type NavTabKey =
  | 'dashboard'
  | 'products'
  | 'warehouses'
  | 'contacts'
  | 'purchase-orders'
  | 'outbound-orders'
  | 'stock-transfers'
  | 'all-vouchers'
  | 'inventory'
  | 'reports'
  | 'history'
  | 'templates'
  | 'users'
  | 'settings';
