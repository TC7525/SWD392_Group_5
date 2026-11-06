using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Models.DTOs;

namespace backend.Services
{
    public interface IInventoryService
    {
        Task<List<InventoryStockResponse>> GetStockOverviewAsync();
        Task<List<InventoryTransactionResponse>> GetRecentTransactionsAsync(int count = 50);
    }
}
