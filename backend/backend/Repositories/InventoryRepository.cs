using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.Models.Entities;

namespace backend.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly AppDbContext _context;

        public InventoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Inventory?> GetByProductAndWarehouseAsync(int productId, int warehouseId)
        {
            return await _context.Inventories
                .Include(i => i.Product)
                .Include(i => i.Warehouse)
                .FirstOrDefaultAsync(i => i.ProductId == productId && i.WarehouseId == warehouseId);
        }

        public async Task<int> GetQuantityAsync(int productId, int warehouseId)
        {
            var item = await _context.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == productId && i.WarehouseId == warehouseId);
            return item?.Quantity ?? 0;
        }

        public async Task<Inventory> IncreaseStockAsync(int productId, int warehouseId, int quantity)
        {
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == productId && i.WarehouseId == warehouseId);

            if (inventory == null)
            {
                inventory = new Inventory
                {
                    ProductId = productId,
                    WarehouseId = warehouseId,
                    Quantity = quantity,
                    LastUpdatedAt = DateTime.UtcNow
                };
                await _context.Inventories.AddAsync(inventory);
            }
            else
            {
                inventory.Quantity += quantity;
                inventory.LastUpdatedAt = DateTime.UtcNow;
                _context.Inventories.Update(inventory);
            }

            await _context.SaveChangesAsync();
            return inventory;
        }

        public Task<Inventory> IncreaseAsync(int productId, int warehouseId, int quantity)
        {
            return IncreaseStockAsync(productId, warehouseId, quantity);
        }

        public async Task<List<Inventory>> GetAllWithDetailsAsync()
        {
            return await _context.Inventories
                .Include(i => i.Product)
                .Include(i => i.Warehouse)
                .OrderBy(i => i.Product!.ProductName)
                .ThenBy(i => i.Warehouse!.WarehouseName)
                .ToListAsync();
        }

        public async Task<List<Inventory>> GetByWarehouseAsync(int warehouseId)
        {
            return await _context.Inventories
                .Include(i => i.Product)
                .Include(i => i.Warehouse)
                .Where(i => i.WarehouseId == warehouseId)
                .OrderBy(i => i.Product!.ProductName)
                .ToListAsync();
        }

        public async Task<Inventory> AddAsync(Inventory inventory)
        {
            await _context.Inventories.AddAsync(inventory);
            await _context.SaveChangesAsync();
            return inventory;
        }

        public async Task UpdateAsync(Inventory inventory)
        {
            _context.Inventories.Update(inventory);
            await _context.SaveChangesAsync();
        }
    }
}
