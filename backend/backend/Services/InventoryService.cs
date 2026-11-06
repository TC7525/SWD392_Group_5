using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Models.DTOs;
using backend.Repositories;

namespace backend.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IInventoryTransactionRepository _transactionRepository;

        public InventoryService(
            IInventoryRepository inventoryRepository,
            IInventoryTransactionRepository transactionRepository)
        {
            _inventoryRepository = inventoryRepository;
            _transactionRepository = transactionRepository;
        }

        public async Task<List<InventoryStockResponse>> GetStockOverviewAsync()
        {
            var inventories = await _inventoryRepository.GetAllWithDetailsAsync();
            return inventories.Select(i => new InventoryStockResponse
            {
                InventoryId = i.InventoryId,
                ProductId = i.ProductId,
                ProductCode = i.Product?.ProductCode ?? string.Empty,
                ProductName = i.Product?.ProductName ?? string.Empty,
                WarehouseId = i.WarehouseId,
                WarehouseCode = i.Warehouse?.WarehouseCode ?? string.Empty,
                WarehouseName = i.Warehouse?.WarehouseName ?? string.Empty,
                Quantity = i.Quantity,
                LastUpdatedAt = i.LastUpdatedAt
            }).ToList();
        }

        public async Task<List<InventoryTransactionResponse>> GetRecentTransactionsAsync(int count = 50)
        {
            var transactions = await _transactionRepository.GetAllAsync(count);
            return transactions.Select(t => new InventoryTransactionResponse
            {
                InventoryTransactionId = t.InventoryTransactionId,
                ProductId = t.ProductId,
                ProductCode = t.Product?.ProductCode ?? string.Empty,
                ProductName = t.Product?.ProductName ?? string.Empty,
                WarehouseId = t.WarehouseId,
                WarehouseCode = t.Warehouse?.WarehouseCode ?? string.Empty,
                WarehouseName = t.Warehouse?.WarehouseName ?? string.Empty,
                UserId = t.UserId,
                UserName = t.User?.FullName ?? string.Empty,
                PurchaseOrderItemId = t.PurchaseOrderItemId,
                PurchaseOrderCode = t.PurchaseOrderItem?.PurchaseOrder?.PurchaseOrderCode,
                TransactionType = t.TransactionType,
                Quantity = t.Quantity,
                TransactionDate = t.TransactionDate,
                Note = t.Note
            }).ToList();
        }
    }
}
