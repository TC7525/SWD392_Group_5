using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Enums;
using backend.Models.Entities;

namespace backend.Repositories
{
    public interface IPurchaseOrderRepository
    {
        Task<PurchaseOrder?> GetByIdAsync(int id);
        Task<PurchaseOrder?> GetByCodeAsync(string code);
        Task<List<PurchaseOrder>> GetAllAsync(POStatus? status = null);
        Task<List<PurchaseOrder>> GetByStatusAsync(POStatus status);
        Task<PurchaseOrder> AddAsync(PurchaseOrder purchaseOrder);
        Task UpdateAsync(PurchaseOrder purchaseOrder);
        Task<bool> ExistsCodeAsync(string code);
        Task<int> GetCountAsync();
    }
}
