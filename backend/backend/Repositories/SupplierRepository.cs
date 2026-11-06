using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.Models.Entities;

namespace backend.Repositories
{
    public class SupplierRepository : ISupplierRepository
    {
        private readonly AppDbContext _context;

        public SupplierRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Supplier?> GetByIdAsync(int id)
        {
            return await _context.Suppliers.FirstOrDefaultAsync(s => s.SupplierId == id);
        }

        public async Task<Supplier?> GetByCodeAsync(string code)
        {
            return await _context.Suppliers.FirstOrDefaultAsync(s => s.SupplierCode == code);
        }

        public async Task<List<Supplier>> GetAllAsync()
        {
            return await _context.Suppliers
                .OrderBy(s => s.SupplierName)
                .ToListAsync();
        }

        public async Task<List<Supplier>> GetActiveSuppliersAsync()
        {
            return await _context.Suppliers
                .Where(s => s.Status == "Active")
                .OrderBy(s => s.SupplierName)
                .ToListAsync();
        }

        public async Task<Supplier> AddAsync(Supplier supplier)
        {
            await _context.Suppliers.AddAsync(supplier);
            await _context.SaveChangesAsync();
            return supplier;
        }

        public async Task UpdateAsync(Supplier supplier)
        {
            _context.Suppliers.Update(supplier);
            await _context.SaveChangesAsync();
        }
    }
}
