using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.Models.Entities;

namespace backend.Repositories
{
    public class InventoryTransactionRepository : IInventoryTransactionRepository
    {
        private readonly AppDbContext _context;

        public InventoryTransactionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<InventoryTransaction> AddAsync(InventoryTransaction transaction)
        {
            await _context.InventoryTransactions.AddAsync(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task<List<InventoryTransaction>> GetAllAsync(int limit = 100)
        {
            return await _context.InventoryTransactions
                .Include(t => t.Product)
                .Include(t => t.Warehouse)
                .Include(t => t.User)
                .Include(t => t.PurchaseOrderItem)
                    .ThenInclude(i => i!.PurchaseOrder)
                .OrderByDescending(t => t.TransactionDate)
                .ThenByDescending(t => t.InventoryTransactionId)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<InventoryTransaction>> GetByProductAsync(int productId)
        {
            return await _context.InventoryTransactions
                .Include(t => t.Product)
                .Include(t => t.Warehouse)
                .Include(t => t.User)
                .Where(t => t.ProductId == productId)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<List<InventoryTransaction>> GetByWarehouseAsync(int warehouseId)
        {
            return await _context.InventoryTransactions
                .Include(t => t.Product)
                .Include(t => t.Warehouse)
                .Include(t => t.User)
                .Where(t => t.WarehouseId == warehouseId)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }
    }
}
