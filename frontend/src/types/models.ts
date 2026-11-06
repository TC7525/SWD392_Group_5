export type POStatus =
  | 'Pending'
  | 'Approved'
  | 'Rejected'
  | 'Processing'
  | 'PartiallyReceived'
  | 'Completed';

export const PO_STATUS = {
  PENDING: 'Pending' as const,
  APPROVED: 'Approved' as const,
  REJECTED: 'Rejected' as const,
  PROCESSING: 'Processing' as const,
  PARTIALLY_RECEIVED: 'PartiallyReceived' as const,
  COMPLETED: 'Completed' as const,
};

export interface ApiResponse<T = any> {
  code: string;
  message: string;
  data: T;
}

export interface User {
  userId: number;
  employeeCode: string;
  fullName: string;
  email: string;
  phone?: string | null;
  role: string;
  roles: string[];
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  user: User;
}

export interface SupplierOptionDto {
  supplierId: number;
  supplierCode: string;
  supplierName: string;
  status: string;
  contactName?: string | null;
  email?: string | null;
  phone?: string | null;
  address?: string | null;
}

export type Supplier = SupplierOptionDto;

export interface ProductOptionDto {
  productId: number;
  productCode: string;
  productName: string;
  unitPrice: number;
  minimumStockLevel: number;
  status: string;
  categoryName?: string | null;
}

export type Product = ProductOptionDto;

export interface Warehouse {
  warehouseId: number;
  warehouseCode: string;
  warehouseName: string;
  location?: string | null;
  status?: string | null;
}

export interface CreatePurchaseOrderItemRequest {
  productId: number;
  orderedQuantity: number;
  unitPrice: number;
}

export interface CreatePurchaseOrderRequest {
  supplierId: number;
  expectedDeliveryDate?: string | null;
  items: CreatePurchaseOrderItemRequest[];
}

export interface PurchaseOrderItemResponse {
  purchaseOrderItemId: number;
  purchaseOrderId: number;
  productId: number;
  productCode?: string | null;
  productName?: string | null;
  orderedQuantity: number;
  receivedQuantity: number;
  unitPrice: number;
  remainingQuantity: number;
  lineTotal: number;
}

export type PurchaseOrderItem = PurchaseOrderItemResponse;

export interface PurchaseOrderResponse {
  purchaseOrderId: number;
  purchaseOrderCode: string;
  supplierId: number;
  supplierCode?: string | null;
  supplierName?: string | null;
  createdBy: number;
  createdByName?: string | null;
  approvedBy?: number | null;
  approvedByName?: string | null;
  status: POStatus | number | string;
  statusName: string;
  orderDate: string;
  expectedDeliveryDate?: string | null;
  approvedDate?: string | null;
  completedDate?: string | null;
  rejectionReason?: string | null;
  createdAt: string;
  updatedAt?: string | null;
  items: PurchaseOrderItemResponse[];
  totalOrderedQuantity?: number;
  totalReceivedQuantity?: number;
  totalRemainingQuantity?: number;
  grandTotal?: number;
}

export type PurchaseOrder = PurchaseOrderResponse;

export interface PurchaseOrderListResponse {
  purchaseOrderId: number;
  purchaseOrderCode: string;
  supplierId: number;
  supplierName: string;
  createdBy: number;
  createdByName: string;
  approvedBy?: number | null;
  approvedByName?: string | null;
  status: POStatus | number | string;
  statusName: string;
  orderDate: string;
  expectedDeliveryDate?: string | null;
  totalItems: number;
  totalQuantity: number;
  totalAmount: number;
  grandTotal: number;
  totalReceivedQuantity?: number;
  totalRemainingQuantity?: number;
  createdAt: string;
}

export interface ReviewPurchaseOrderRequest {
  reason?: string | null;
}

export interface CategoryOptionDto {
  categoryId: number;
  categoryName: string;
}

export interface QuickCreateProductRequest {
  productCode?: string;
  productName: string;
  categoryId?: number;
  brand?: string;
  unitPrice: number;
  description?: string;
}

export interface PurchaseOrderFormDataResponse {
  suppliers: SupplierOptionDto[];
  products: ProductOptionDto[];
  categories?: CategoryOptionDto[];
}

export interface CheckQuantityItemRequest {
  productId: number;
  quantity: number;
}

export interface CheckQuantityItemResult {
  productId: number;
  productCode?: string | null;
  productName?: string | null;
  requestedQuantity: number;
  quantity: number;
  remainingQuantity: number;
  isValid: boolean;
  message?: string | null;
}

export interface CheckQuantityResponse {
  valid: boolean;
  message?: string | null;
  items: CheckQuantityItemResult[];
}

export interface ConfirmReceiveItemRequest {
  purchaseOrderItemId: number;
  receivedQuantity: number;
  quantity?: number;
  productId?: number | null;
}

export interface ConfirmReceiveRequest {
  warehouseId: number;
  note?: string | null;
  items: ConfirmReceiveItemRequest[];
}

export interface InventoryStockResponse {
  inventoryId: number;
  productId: number;
  productCode: string;
  productName: string;
  warehouseId: number;
  warehouseCode: string;
  warehouseName: string;
  quantity: number;
  minimumStockLevel?: number;
  lastUpdatedAt: string;
}

export type InventoryItem = InventoryStockResponse;

export interface InventoryTransactionResponse {
  inventoryTransactionId: number;
  productId: number;
  productCode: string;
  productName: string;
  warehouseId: number;
  warehouseCode: string;
  warehouseName: string;
  userId: number;
  userName: string;
  purchaseOrderItemId?: number | null;
  purchaseOrderCode?: string | null;
  transactionType: string | number;
  transactionTypeName: string;
  quantity: number;
  transactionDate: string;
  note?: string | null;
}

export interface ApiError {
  code: string;
  message: string;
  status?: number;
  errors?: Record<string, string[]>;
}
