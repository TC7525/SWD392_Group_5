using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Models.DTOs;

namespace backend.Services
{
    public interface IReceivingService
    {
        Task<List<PurchaseOrderListResponse>> GetReceivableOrdersAsync();
        Task<CheckQuantityResponse> CheckQuantityAsync(int poId, List<CheckQuantityItemRequest> items);
        Task<PurchaseOrderResponse> StartReceivingAsync(int poId, int staffId);
        Task<PurchaseOrderResponse> CancelReceivingAsync(int poId, int staffId);
        Task<PurchaseOrderResponse> ConfirmReceiveAsync(int poId, ConfirmReceiveRequest request, int staffId);
    }
}
