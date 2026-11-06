using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Models.Entities;

namespace backend.Repositories
{
    public interface IInventoryRepository
    {
        Task<Inventory?> GetByProductAndWarehouseAsync(int productId, int warehouseId);
        Task<int> GetQuantityAsync(int productId, int warehouseId);
        Task<Inventory> IncreaseStockAsync(int productId, int warehouseId, int quantity);
        Task<Inventory> IncreaseAsync(int productId, int warehouseId, int quantity);
        Task<List<Inventory>> GetAllWithDetailsAsync();
        Task<List<Inventory>> GetByWarehouseAsync(int warehouseId);
        Task<Inventory> AddAsync(Inventory inventory);
        Task UpdateAsync(Inventory inventory);
    }
}
