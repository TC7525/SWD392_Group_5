import apiClient from './client';
import type {
  ApiResponse,
  CreatePurchaseOrderRequest,
  PurchaseOrderFormDataResponse,
  PurchaseOrderListResponse,
  PurchaseOrderResponse,
  POStatus,
  ReviewPurchaseOrderRequest,
} from '../types/models';

export const poApi = {
  getAll: async (status?: POStatus | string): Promise<ApiResponse<PurchaseOrderListResponse[]>> => {
    const params = status ? { status } : undefined;
    const res = await apiClient.get<any, ApiResponse<PurchaseOrderListResponse[]>>('/purchase-orders', {
      params,
    });
    return res;
  },

  getById: async (id: number): Promise<ApiResponse<PurchaseOrderResponse>> => {
    const res = await apiClient.get<any, ApiResponse<PurchaseOrderResponse>>(`/purchase-orders/${id}`);
    return res;
  },

  getPending: async (): Promise<ApiResponse<PurchaseOrderListResponse[]>> => {
    const res = await apiClient.get<any, ApiResponse<PurchaseOrderListResponse[]>>('/purchase-orders/pending');
    return res;
  },

  getFormData: async (): Promise<ApiResponse<PurchaseOrderFormDataResponse>> => {
    const res = await apiClient.get<any, ApiResponse<PurchaseOrderFormDataResponse>>('/purchase-orders/form-data');
    return res;
  },

  create: async (data: CreatePurchaseOrderRequest): Promise<ApiResponse<PurchaseOrderResponse>> => {
    const res = await apiClient.post<any, ApiResponse<PurchaseOrderResponse>>('/purchase-orders', data);
    return res;
  },

  approve: async (id: number): Promise<ApiResponse<PurchaseOrderResponse>> => {
    const res = await apiClient.post<any, ApiResponse<PurchaseOrderResponse>>(`/purchase-orders/${id}/approve`);
    return res;
  },

  reject: async (id: number, reason?: string): Promise<ApiResponse<PurchaseOrderResponse>> => {
    const payload: ReviewPurchaseOrderRequest = { reason: reason ?? '' };
    const res = await apiClient.post<any, ApiResponse<PurchaseOrderResponse>>(
      `/purchase-orders/${id}/reject`,
      payload
    );
    return res;
  },
};

export default poApi;
