import { type PagedResult } from '../types/auth.types';
import {
  type AccessLogItem,
  type AccountItem,
  type CategoryItem,
  type CustomerItem,
  type ProductItem,
  type RoleItem,
  type SupplierItem,
  type WarehouseItem,
} from '../types/master.types';
import { httpRequest } from './httpClient';

export const masterDataService = {
  // Warehouses
  searchWarehouses: (keyword?: string) =>
    httpRequest<PagedResult<WarehouseItem>>('api/warehouse/search', {
      method: 'POST',
      body: JSON.stringify({
        keyword,
        page_index: 1,
        page_size: 100,
      }),
    }),

  createWarehouse: (payload: {
    warehouse_code?: string;
    warehouse_name: string;
    address?: string;
    status?: number;
  }) =>
    httpRequest<string>('api/warehouse/create', {
      method: 'POST',
      body: JSON.stringify({ status: 1, ...payload }),
    }),

  updateWarehouse: (payload: {
    id: string;
    warehouse_code?: string;
    warehouse_name: string;
    address?: string;
    status?: number;
  }) =>
    httpRequest<boolean>('api/warehouse/update', {
      method: 'PUT',
      body: JSON.stringify({ status: 1, ...payload }),
    }),

  deleteWarehouse: (id: string) =>
    httpRequest<boolean>('api/warehouse/delete', {
      method: 'POST',
      body: JSON.stringify({ id }),
    }),

  // Products
  searchProducts: (warehouse_id?: string, keyword?: string) =>
    httpRequest<PagedResult<ProductItem>>('api/product/search', {
      method: 'POST',
      body: JSON.stringify({
        warehouse_id,
        keyword,
        page_index: 1,
        page_size: 100,
      }),
    }),

  createProduct: (payload: {
    product_code?: string;
    product_name: string;
    category_id: string;
    brand?: string;
    description?: string;
    unit_price: number;
    minimum_stock_level: number;
  }) =>
    httpRequest<string>('api/product/create', {
      method: 'POST',
      body: JSON.stringify(payload),
    }),

  updateProduct: (payload: {
    id: string;
    product_code?: string;
    product_name: string;
    category_id: string;
    brand?: string;
    description?: string;
    unit_price: number;
    minimum_stock_level: number;
    status?: number;
  }) =>
    httpRequest<boolean>('api/product/update', {
      method: 'PUT',
      body: JSON.stringify({ status: 1, ...payload }),
    }),

  deleteProduct: (id: string) =>
    httpRequest<boolean>('api/product/delete', {
      method: 'POST',
      body: JSON.stringify({ id }),
    }),

  // Categories
  searchCategories: () =>
    httpRequest<PagedResult<CategoryItem>>('api/category/search', {
      method: 'POST',
      body: JSON.stringify({ page_index: 1, page_size: 100 }),
    }),

  createCategory: (payload: {
    category_code?: string;
    category_name: string;
    description?: string;
  }) =>
    httpRequest<string>('api/category/create', {
      method: 'POST',
      body: JSON.stringify(payload),
    }),

  deleteCategory: (id: string) =>
    httpRequest<boolean>('api/category/delete', {
      method: 'POST',
      body: JSON.stringify({ id }),
    }),

  // Suppliers
  searchSuppliers: (keyword?: string) =>
    httpRequest<PagedResult<SupplierItem>>('api/supplier/search', {
      method: 'POST',
      body: JSON.stringify({ keyword, page_index: 1, page_size: 100 }),
    }),

  createSupplier: (payload: {
    supplier_code?: string;
    supplier_name: string;
    contact_person?: string;
    phone?: string;
    email?: string;
    address?: string;
  }) =>
    httpRequest<string>('api/supplier/create', {
      method: 'POST',
      body: JSON.stringify({ status: 1, ...payload }),
    }),

  updateSupplier: (payload: {
    id: string;
    supplier_code?: string;
    supplier_name: string;
    contact_person?: string;
    phone?: string;
    email?: string;
    address?: string;
    status?: number;
  }) =>
    httpRequest<boolean>('api/supplier/update', {
      method: 'PUT',
      body: JSON.stringify({ status: 1, ...payload }),
    }),

  deleteSupplier: (id: string) =>
    httpRequest<boolean>('api/supplier/delete', {
      method: 'POST',
      body: JSON.stringify({ id }),
    }),

  // Customers
  searchCustomers: (keyword?: string) =>
    httpRequest<PagedResult<CustomerItem>>('api/customer/search', {
      method: 'POST',
      body: JSON.stringify({ keyword, page_index: 1, page_size: 100 }),
    }),

  createCustomer: (payload: {
    customer_code?: string;
    customer_name: string;
    phone?: string;
    email?: string;
    address?: string;
  }) =>
    httpRequest<string>('api/customer/create', {
      method: 'POST',
      body: JSON.stringify({ status: 1, ...payload }),
    }),

  updateCustomer: (payload: {
    id: string;
    customer_code?: string;
    customer_name: string;
    phone?: string;
    email?: string;
    address?: string;
    status?: number;
  }) =>
    httpRequest<boolean>('api/customer/update', {
      method: 'PUT',
      body: JSON.stringify({ status: 1, ...payload }),
    }),

  deleteCustomer: (id: string) =>
    httpRequest<boolean>('api/customer/delete', {
      method: 'POST',
      body: JSON.stringify({ id }),
    }),

  // Roles & Accounts
  searchRoles: () =>
    httpRequest<PagedResult<RoleItem>>('api/role/search', {
      method: 'POST',
      body: JSON.stringify({ page_index: 1, page_size: 50 }),
    }),

  searchAccounts: (keyword?: string) =>
    httpRequest<PagedResult<AccountItem>>('api/account/search', {
      method: 'POST',
      body: JSON.stringify({ keyword, page_index: 1, page_size: 100 }),
    }),

  createAccount: (payload: {
    employee_code?: string;
    full_name: string;
    email: string;
    phone?: string;
    password: string;
    role_id: string;
    warehouse_ids: string[];
    status?: number;
  }) =>
    httpRequest<string>('api/account/create', {
      method: 'POST',
      body: JSON.stringify({ status: 1, ...payload }),
    }),

  updateAccount: (payload: {
    id: string;
    employee_code?: string;
    full_name: string;
    email: string;
    phone?: string;
    role_id: string;
    warehouse_ids: string[];
    status?: number;
  }) =>
    httpRequest<boolean>('api/account/update', {
      method: 'PUT',
      body: JSON.stringify({ status: 1, ...payload }),
    }),

  deleteAccount: (id: string) =>
    httpRequest<boolean>('api/account/delete', {
      method: 'POST',
      body: JSON.stringify({ id }),
    }),

  // Access Logs
  searchAccessLogs: (keyword?: string) =>
    httpRequest<PagedResult<AccessLogItem>>('api/access-log/search', {
      method: 'POST',
      body: JSON.stringify({ keyword, page_index: 1, page_size: 100 }),
    }),
};
