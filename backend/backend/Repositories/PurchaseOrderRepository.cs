using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend.Enums;
using backend.Models;
using backend.Models.Entities;

namespace backend.Repositories
{
    public class PurchaseOrderRepository : IPurchaseOrderRepository
    {
        private readonly AppDbContext _context;

        public PurchaseOrderRepository(AppDbContext context)
        {
            _context = context;
        }

        private IQueryable<PurchaseOrder> QueryWithIncludes()
        {
            return _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.Creator)
                .Include(po => po.Approver)
                .Include(po => po.Items)
                    .ThenInclude(i => i.Product);
        }

        public async Task<PurchaseOrder?> GetByIdAsync(int id)
        {
            return await QueryWithIncludes()
                .FirstOrDefaultAsync(po => po.PurchaseOrderId == id);
        }

        public async Task<PurchaseOrder?> GetByCodeAsync(string code)
        {
            return await QueryWithIncludes()
                .FirstOrDefaultAsync(po => po.PurchaseOrderCode == code);
        }

        public async Task<List<PurchaseOrder>> GetAllAsync(POStatus? status = null)
        {
            var query = QueryWithIncludes();
            if (status.HasValue)
            {
                query = query.Where(po => po.Status == status.Value);
            }
            return await query
                .OrderByDescending(po => po.OrderDate)
                .ThenByDescending(po => po.PurchaseOrderId)
                .ToListAsync();
        }

        public async Task<List<PurchaseOrder>> GetByStatusAsync(POStatus status)
        {
            return await GetAllAsync(status);
        }

        public async Task<PurchaseOrder> AddAsync(PurchaseOrder purchaseOrder)
        {
            await _context.PurchaseOrders.AddAsync(purchaseOrder);
            await _context.SaveChangesAsync();
            return purchaseOrder;
        }

        public async Task UpdateAsync(PurchaseOrder purchaseOrder)
        {
            _context.PurchaseOrders.Update(purchaseOrder);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsCodeAsync(string code)
        {
            return await _context.PurchaseOrders.AnyAsync(po => po.PurchaseOrderCode == code);
        }

        public async Task<int> GetCountAsync()
        {
            return await _context.PurchaseOrders.CountAsync();
        }
    }
}
