import apiClient from './client';
import type {
  ApiResponse,
  CheckQuantityItemRequest,
  CheckQuantityResponse,
  ConfirmReceiveRequest,
  PurchaseOrderListResponse,
  PurchaseOrderResponse,
} from '../types/models';

export const receivingApi = {
  getReceivableOrders: async (): Promise<ApiResponse<PurchaseOrderListResponse[]>> => {
    const res = await apiClient.get<any, ApiResponse<PurchaseOrderListResponse[]>>('/receiving/orders');
    return res;
  },

  checkQuantity: async (
    poId: number,
    items: CheckQuantityItemRequest[]
  ): Promise<ApiResponse<CheckQuantityResponse>> => {
    const res = await apiClient.post<any, ApiResponse<CheckQuantityResponse>>(
      `/receiving/${poId}/check-quantity`,
      items
    );
    return res;
  },

  startReceiving: async (poId: number): Promise<ApiResponse<PurchaseOrderResponse>> => {
    const res = await apiClient.post<any, ApiResponse<PurchaseOrderResponse>>(`/receiving/${poId}/start`);
    return res;
  },

  cancelReceiving: async (poId: number): Promise<ApiResponse<PurchaseOrderResponse>> => {
    const res = await apiClient.post<any, ApiResponse<PurchaseOrderResponse>>(`/receiving/${poId}/cancel`);
    return res;
  },

  confirmReceive: async (
    poId: number,
    request: ConfirmReceiveRequest
  ): Promise<ApiResponse<PurchaseOrderResponse>> => {
    const res = await apiClient.post<any, ApiResponse<PurchaseOrderResponse>>(
      `/receiving/${poId}/confirm`,
      request
    );
    return res;
  },
};

export default receivingApi;
