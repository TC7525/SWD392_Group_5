using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Models.Entities;

namespace backend.Repositories
{
    public interface IInventoryTransactionRepository
    {
        Task<InventoryTransaction> AddAsync(InventoryTransaction transaction);
        Task<List<InventoryTransaction>> GetAllAsync(int limit = 100);
        Task<List<InventoryTransaction>> GetByProductAsync(int productId);
        Task<List<InventoryTransaction>> GetByWarehouseAsync(int warehouseId);
    }
}
