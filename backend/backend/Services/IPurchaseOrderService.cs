using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Enums;
using backend.Models.DTOs;

namespace backend.Services
{
    public interface IPurchaseOrderService
    {
        Task<PurchaseOrderResponse> CreateOrderAsync(CreatePurchaseOrderRequest request, int userId);
        Task<PurchaseOrderResponse> ApproveOrderAsync(int id, int managerId);
        Task<PurchaseOrderResponse> RejectOrderAsync(int id, int managerId, string reason);
        Task<List<PurchaseOrderListResponse>> GetAllAsync(POStatus? status = null);
        Task<List<PurchaseOrderListResponse>> GetPendingOrdersAsync();
        Task<PurchaseOrderResponse> GetOrderDetailAsync(int id);
        Task<PurchaseOrderFormDataResponse> GetFormDataAsync();
    }
}
