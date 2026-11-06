import apiClient from './client';
import type {
  ApiResponse,
  InventoryStockResponse,
  InventoryTransactionResponse,
} from '../types/models';

export const inventoryApi = {
  getStockOverview: async (): Promise<ApiResponse<InventoryStockResponse[]>> => {
    const res = await apiClient.get<any, ApiResponse<InventoryStockResponse[]>>('/inventories');
    return res;
  },

  getRecentTransactions: async (
    count: number = 50
  ): Promise<ApiResponse<InventoryTransactionResponse[]>> => {
    const res = await apiClient.get<any, ApiResponse<InventoryTransactionResponse[]>>(
      '/inventories/transactions',
      {
        params: { count },
      }
    );
    return res;
  },
};

export default inventoryApi;
